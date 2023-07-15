using Microsoft.AspNetCore.SignalR;
using MyNetCore.Dtos.ChatRoom;
using Newtonsoft.Json;
using Roim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Business.Signal
{
    public class MsgType
    {
        /// <summary>
        /// 用户说话
        /// </summary>
        public const string UserSpeak = "userSpeak";
        /// <summary>
        /// 用户进入房间
        /// </summary>
        public const string UserCome = "userCome";
        /// <summary>
        /// 用户离开房间
        /// </summary>
        public const string UserGo = "userGo";
    }

    public class SignalRHub : Hub
    {
        /// <summary>
        /// 聊天室信息
        /// </summary>
        public static List<ChatRoomInfo> ChatRoomInfos = new List<ChatRoomInfo>();

        /// <summary>
        /// 是否启用自动回复
        /// </summary>
        public static Dictionary<string, bool> RunAutoAnswer { get; set; }

        /// <summary>
        /// 自动回复头像
        /// </summary>
        private string _AutoAnswerHeadImg = "/Content/images/header/autoAnswer.png";

        /// <summary>
        /// 自动回复用户
        /// </summary>
        private string _AutoAnswerUser = "自动回复";

        public async Task sendall(string message, string recordId, string type)
        {
            BusinessChatRoom businessChatRoom = new BusinessChatRoom();

            var currentUser = businessChatRoom.GetCurrentUserInfo();

            switch (type)
            {
                case MsgType.UserSpeak:
                    await Clients.All.SendAsync("toall_" + recordId, currentUser?.Name, currentUser?.Code, currentUser.HeadImage, message, type);

                    if (RunAutoAnswer == null)
                    {
                        RunAutoAnswer = new Dictionary<string, bool>();
                    }

                    if (!RunAutoAnswer.Keys.Contains(recordId))
                    {
                        RunAutoAnswer.Add(recordId, false);
                    }

                    if (message?.Contains("@@") == true)
                    {
                        if (!SignalRHub.ChatRoomInfos.Any(m => m.RoomId == recordId && m.UserCode == _AutoAnswerUser))
                        {
                            SignalRHub.ChatRoomInfos.Add(new ChatRoomInfo()
                            {
                                RoomId = recordId,
                                UserCode = _AutoAnswerUser,
                                UserName = _AutoAnswerUser,
                                UserHeadImg = _AutoAnswerHeadImg
                            });
                            string reMessage = JsonConvert.SerializeObject(new { UserCode = _AutoAnswerUser, UserName = _AutoAnswerUser, UserHeadImg = _AutoAnswerHeadImg });
                            await Clients.All.SendAsync("toall_" + recordId, _AutoAnswerUser, _AutoAnswerUser, _AutoAnswerHeadImg, reMessage, MsgType.UserCome);
                        }

                        RunAutoAnswer[recordId] = true;
                        await Clients.All.SendAsync("toall_" + recordId, _AutoAnswerUser, _AutoAnswerUser, _AutoAnswerHeadImg, "开启了自动回复", type);
                    }
                    else if (message?.Contains("!@") == true)
                    {
                        RunAutoAnswer[recordId] = false;
                        await Clients.All.SendAsync("toall_" + recordId, _AutoAnswerUser, _AutoAnswerUser, _AutoAnswerHeadImg, "关闭了自动回复", type);

                        var temp2 = ChatRoomInfos.FirstOrDefault(m => m.RoomId == recordId && m.UserCode == _AutoAnswerUser);
                        if (temp2 != null)
                        {
                            ChatRoomInfos.Remove(temp2);
                            string reMessage = _AutoAnswerUser;
                            await Clients.All.SendAsync("toall_" + recordId, currentUser?.Name, currentUser?.Code, currentUser.HeadImage, reMessage, MsgType.UserGo);
                        }
                    }

                    if (RunAutoAnswer[recordId])
                    {
                        message = message.Replace("@@", "");

                        if (!string.IsNullOrWhiteSpace(message))
                        {
                            string answer = WebClientCustom.CreateHttpResponseByHttpClient($"http://api.qingyunke.com/api.php?key=free&appid=0&msg={message.Replace("@@", "")}", false);
                            BotAnswerInfo tm = JsonConvert.DeserializeObject<BotAnswerInfo>(answer);
                            await Clients.All.SendAsync("toall_" + recordId, _AutoAnswerUser, _AutoAnswerUser, _AutoAnswerHeadImg, $"@{currentUser?.Name} {tm?.Content?.Replace("{br}", "<br/>")}", type);
                        }
                    }

                    break;
                case MsgType.UserCome:
                    BusinessUsers businessUsers = new BusinessUsers();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        string sessionId = AppContextMy.Current.Request.Query["id"];

                        ChatRoomInfo chatRoomInfo = SignalRHub.ChatRoomInfos?.FirstOrDefault(m => m.SessionId == sessionId);

                        if (!string.IsNullOrWhiteSpace(message) && chatRoomInfo != null)
                        {
                            if (currentUser != null)
                            {
                                chatRoomInfo.RoomId = recordId;
                                chatRoomInfo.UserCode = currentUser.Code;
                                chatRoomInfo.UserName = currentUser.Name;
                                chatRoomInfo.UserHeadImg = currentUser.HeadImage;
                                message = JsonConvert.SerializeObject(new { UserCode = currentUser.Code, UserName = currentUser.Name, UserHeadImg = currentUser.HeadImage });
                                await Clients.All.SendAsync("toall_" + recordId, currentUser?.Name, currentUser?.Code, currentUser.HeadImage, message, type);
                            }
                        }
                    }
                    break;
                case MsgType.UserGo:
                    //var temp = ChatRoomInfos.FirstOrDefault(m => m.RoomId == recordId && m.UserCode == message);
                    //if (temp != null)
                    //{
                    //    ChatRoomInfos.Remove(temp);
                    //    message = currentUser.Code;
                    //    await Clients.All.SendAsync("toall_" + recordId, currentUser?.Name, currentUser?.Code, currentUser.HeadImage, message, type);
                    //}
                    break;
                default:
                    await Clients.All.SendAsync("toall_" + recordId, currentUser?.Name, currentUser?.Code, currentUser.HeadImage, message, type);
                    break;
            }


        }

        public async Task SendToUser(string user, string message)
        {
            BusinessChatRoom businessChatRoom = new BusinessChatRoom();

            var currentUser = businessChatRoom.GetCurrentUserInfo();

            //给指定人推送消息			
            await Clients.Client(user).SendAsync("touser", $"{currentUser?.Name}({currentUser?.Code})#{currentUser.HeadImage}", message, "只给你发");
        }

        /// <summary>		
        /// 重写集线器连接事件		
        /// </summary>		
        /// <returns></returns>		
        public override Task OnConnectedAsync()
        {
            string sessionId = AppContextMy.Current.Request.Query["id"];

            if (!ChatRoomInfos.Any(m => m.SessionId == sessionId))
            {
                SignalRHub.ChatRoomInfos.Add(new ChatRoomInfo()
                {
                    SessionId = sessionId,
                    ConnectionId = Context.ConnectionId
                });
            }

            Console.WriteLine($"{Context.ConnectionId}已连接");

            return base.OnConnectedAsync();
        }
        /// <summary>		
        /// 重写集线器关闭事件		
        /// </summary>		
        /// <param name="exception"></param>		
        /// <returns></returns>		
        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine("触发了关闭事件");
            var temp = ChatRoomInfos.FirstOrDefault(m => m.ConnectionId == Context.ConnectionId);
            if (temp != null)
            {
                ChatRoomInfos.Remove(temp);
                Clients.All.SendAsync("toall_" + temp.RoomId, temp.UserName, temp.UserCode, temp.UserHeadImg, temp.UserCode, MsgType.UserGo);
            }
            return base.OnDisconnectedAsync(exception);
        }

    }
}
