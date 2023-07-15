using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyNetCore.Dtos.ChatRoom
{
    /// <summary>
    /// 房间信息
    /// </summary>
    public class ChatRoomInfo
    {
        /// <summary>
        /// 房间ID
        /// </summary>
        public string RoomId { get; set; }
        /// <summary>
        /// 连接ID
        /// </summary>
        public string ConnectionId { get; set; }
        /// <summary>
        /// 回话ID
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// 用户编码
        /// </summary>
        public string UserCode { get; set; }
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 用户头像
        /// </summary>
        public string UserHeadImg { get; set; }
    }
}
