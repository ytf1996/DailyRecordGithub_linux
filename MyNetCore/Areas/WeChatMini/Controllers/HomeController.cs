using Microsoft.AspNetCore.Mvc;
using MyNetCore.Business;
using MyNetCore.Models;
using Newtonsoft.Json.Linq;
using Roim.Common;
using System.Text.RegularExpressions;

namespace MyNetCore.Areas.WeChatMini.Controllers
{
    public class HomeController : WeChatMiniBaseWithAuthController
    {
        public IActionResult GetMyInfo()
        {
            var currentUser = GetCurrentUserInfo();
            return Success(data: new { code = currentUser.Code, name = currentUser.Name, phone = currentUser.PhoneNum });
        }

        /// <summary>
        /// 修改用户信息
        /// </summary>
        /// <param name="code">登录名</param>
        /// <param name="password">密码</param>
        /// <param name="phoneNum">手机号</param>
        /// <returns></returns>
        public IActionResult UpdateUserInfo(string code, string password, string phoneNum)
        {
            BusinessUsers businessUsers = new BusinessUsers();

            businessUsers.UpdateUserInfo(code, password, phoneNum);

            return Success();
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public IActionResult UpdatePassword(string password)
        {
            BusinessUsers businessUsers = new BusinessUsers();

            var currentUser = GetCurrentUserInfo();

            businessUsers.UpdatePassword(currentUser.Code, password, currentUser.Id);

            return Success();
        }

        public IActionResult Weather(string city)
        {
            if (string.IsNullOrWhiteSpace(city))
            {
                city = "上海";
            }
            string url = string.Format("https://v0.yiketianqi.com/api?version=v61&appid=63399175&appsecret=tDen62PL&city={0}", city);
            string result = new WebClientCustom().CreateHttpResponseForGzip(url);
            result = UnicodeDencode(result);
            return Success(data: result);
        }

        /// <summary>  
        /// 转换输入字符串中的任何转义字符。如：Unicode 的中文 \u8be5  
        /// </summary>  
        /// <param name="str"></param>  
        /// <returns></returns>  
        public string UnicodeDencode(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return str;
            return Regex.Unescape(str);
        }

        /// <summary>
        /// 根据经纬度获得当前地址信息
        /// </summary>
        /// <returns></returns>
        public IActionResult GetInfoByLatAndLng(string lat, string lng)
        {
            if (string.IsNullOrWhiteSpace(lat) || string.IsNullOrWhiteSpace(lng))
            {
                return Json("");
            }

            BusinessLatLngHistory businessLatLngHistory = new BusinessLatLngHistory();

            LatLngHistory latLngHistory = new LatLngHistory()
            {
                Lat = lat,
                Lng = lng
            };

            businessLatLngHistory.Add(latLngHistory, false);

            string url = string.Format("http://api.map.baidu.com/geocoder/v2/?location={0},{1}&output=json&pois=1&ak=tDm05PF7lev0B34wezbfG2GU",
                lat, lng);
            string result = new WebClientCustom().CreateHttpResponse(url);

            return Success(data: result);
        }

        public IActionResult GetWeatherByLatAndLng(string lat, string lng)
        {
            if (string.IsNullOrWhiteSpace(lat) || string.IsNullOrWhiteSpace(lng))
            {
                lat = "22.54286";
                lng = "114.05956";
            }

            BusinessLatLngHistory businessLatLngHistory = new BusinessLatLngHistory();

            LatLngHistory latLngHistory = new LatLngHistory()
            {
                Lat = lat,
                Lng = lng
            };

            businessLatLngHistory.Add(latLngHistory, false);

            string url = string.Format("http://api.map.baidu.com/geocoder/v2/?location={0},{1}&output=json&pois=1&ak=tDm05PF7lev0B34wezbfG2GU",
                lat, lng);
            string result = new WebClientCustom().CreateHttpResponse(url);

            var weatherData = JObject.Parse(result);

            string city = string.Empty;

            try
            {
                city = (((Newtonsoft.Json.Linq.JValue)weatherData["result"]["addressComponent"]["city"]).Value).ToString();

                if (city[city.Length - 1] == '市')
                {
                    city = city.Substring(0, city.Length - 1);
                }
            }
            catch
            {
                city = "上海";
            }
            

            return Weather(city);
        }
    }
}
