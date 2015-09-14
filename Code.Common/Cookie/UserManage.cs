using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Common.Cookie
{
    public class UserManage
    {
        public static string MakeUserInfoCookie(UserCookieObject user)
        {
            if (user == null || string.IsNullOrEmpty(user.LoginName))
                return string.Empty;
            StringBuilder data = new StringBuilder();
            if (!string.IsNullOrEmpty(user.UserID))
            {
                data.Append("UD=");
                data.Append(user.UserID.ToString());
            }

            if (!string.IsNullOrEmpty(user.LoginName))
            {
                data.Append(";LN=");
                data.Append(UserCookieEncrypt.CookieEncode(user.LoginName));
            }

            if (!string.IsNullOrEmpty(user.UserName))
            {
                data.Append(";UN=");
                data.Append(UserCookieEncrypt.CookieEncode(user.UserName));
            }

            if (!string.IsNullOrEmpty(user.NickName))
            {
                data.Append(";NN=");
                data.Append(UserCookieEncrypt.CookieEncode(user.NickName));
            }
            if (!string.IsNullOrEmpty(user.LoginBUID))
            {
                data.Append(";BD=");
                data.Append(user.LoginBUID.ToString());
            }

            if (!string.IsNullOrEmpty(user.LoginBUName))
            {
                data.Append(";BN=");
                data.Append(UserCookieEncrypt.CookieEncode(user.LoginBUName));
            }
            if (!string.IsNullOrEmpty(user.TokenMask))
            {
                data.Append(";TM=");
                data.Append(UserCookieEncrypt.CookieEncode(user.TokenMask));
            }

            if (!string.IsNullOrEmpty(user.Email))
            {
                data.Append(";EM=");
                data.Append(UserCookieEncrypt.CookieEncode(user.Email));
            }

            if (!string.IsNullOrEmpty(user.Status))
            {
                data.Append(";ST=");
                data.Append(user.Status);
            }

            data.Append(";AT=");
            DateTime now = DateTime.Now;
            data.Append(UserCookieEncrypt.CookieEncode(string.Format("{0}-{1} {2}:{3}", now.Month.ToString(), now.Day.ToString(), now.Hour.ToString(), now.Minute.ToString())));

            if (!string.IsNullOrEmpty(user.DepartmentID))
            {
                data.Append(";DD=");
                data.Append(user.DepartmentID.ToString());
            }

            if (!string.IsNullOrEmpty(user.DepartmentName))
            {
                data.Append(";DN=");
                data.Append(UserCookieEncrypt.CookieEncode(user.DepartmentName));
            }

            if (!string.IsNullOrEmpty(user.LevelMark))
            {
                data.Append(";LM=");
                data.Append(UserCookieEncrypt.CookieEncode(user.LevelMark));
            }

            if (!string.IsNullOrEmpty(user.CityID))
            {
                data.Append(";CD=");
                data.Append(user.CityID.ToString());
            }

            if (user.IsAutoLogin != null)
            {
                data.Append(";AL=");
                data.Append(user.IsAutoLogin ? "1" : "0");
            }
            else
                data.Append(";AL=0");

            if (!string.IsNullOrEmpty(user.UAccountNo))
            {
                data.Append(";UA=");
                data.Append(UserCookieEncrypt.CookieEncode(user.UAccountNo));
            }

            if (!string.IsNullOrEmpty(user.CAccountNo))
            {
                data.Append(";CA=");
                data.Append(UserCookieEncrypt.CookieEncode(user.CAccountNo));
            }

            if (!string.IsNullOrEmpty(user.DAccountNo))
            {
                data.Append(";DA=");
                data.Append(UserCookieEncrypt.CookieEncode(user.DAccountNo));
            }

            if (!string.IsNullOrEmpty(user.UserType))
            {
                data.Append(";UT=");
                data.Append(user.UserType.ToString());
            }

            if (!string.IsNullOrEmpty(user.UserSourceID))
            {
                data.Append(";SI=");
                data.Append(user.UserSourceID.ToString());
            }

            if (!string.IsNullOrEmpty(user.DepLoginCount))
            {
                data.Append(";DC=");
                data.Append(user.DepLoginCount.ToString());
            }
            if (!string.IsNullOrEmpty(user.UserRoleId))
            {
                data.Append(";RI=");
                data.Append(user.UserRoleId);
            }
            data.Append(";");
            return data.ToString();
        }

        /// <summary>
        /// 将加密用户信息Cookie内容生成UserCookieObject对象
        /// </summary>
        /// <param name="userCookieEncrypt">密用户信息Cookie</param>
        /// <returns></returns>
        public static UserCookieObject GetUserInfoByCookie(string userCookieEncrypt)
        {
            if (string.IsNullOrEmpty(userCookieEncrypt))
                return null;

            string cookieValue = UserCookieEncrypt.DecryptData(userCookieEncrypt);
            if (string.IsNullOrEmpty(cookieValue))
                return null;
            return GetUserInfoCookie(cookieValue);
        }

        //public static UserCookieObject GetUserInfoCookie(UserInfo uinfo)
        //{
        //    if (uinfo == null) return null;
        //    UserCookieObject user = new UserCookieObject();
        //    user.UserID = uinfo.Usermaster_Id;
        //    user.UserName = uinfo.Usermaster_Name;
        //    user.LoginName = uinfo.Login_Name;
        //    //user.NickName = uinfo.Work_Years_Range_Id;
        //    //user.LoginBUID = uinfo.;
        //    user.Email = uinfo.Email;
        //    user.Status = ((int)uinfo.Status).ToString();
        //    user.UAccountNo = uinfo.Usermaster_Ext_Id;
        //    user.UserSourceID = ((int)uinfo.Usermaster_Source_Id).ToString();
        //    return user;
        //}

        /// <summary>
        /// 将解密后的用户信息Cookie内容生成UserCookieObject对象
        /// </summary>
        /// <param name="userCookieValue">解密后的用户信息Cookie值</param>
        /// <returns></returns>
        public static UserCookieObject GetUserInfoCookie(string userCookieValue)
        {
            if (string.IsNullOrEmpty(userCookieValue))
                return null;
            UserCookieObject user = new UserCookieObject();
            string[] arrUserInfo = userCookieValue.Split(';');
            foreach (string item in arrUserInfo)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                string[] arrItem = item.Split('=');
                if (arrItem.Length == 2)
                {
                    switch (arrItem[0].ToUpper())
                    {
                        case "UD":
                            user.UserID = arrItem[1].ToString();
                            break;
                        case "LN":
                            user.LoginName = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "UN":
                            user.UserName = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "NN":
                            user.NickName = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "BD":
                            user.LoginBUID = arrItem[1].ToString();
                            break;
                        case "BN":
                            user.LoginBUName = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "TM":
                            user.TokenMask = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "EM":
                            user.Email = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "ST":
                            user.Status = arrItem[1].ToString();
                            break;
                        case "AT":
                            user.ActionTime = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "DD":
                            user.DepartmentID = arrItem[1].ToString();
                            break;
                        case "DN":
                            user.DepartmentName = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "LM":
                            user.LevelMark = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "CD":
                            user.CityID = arrItem[1].ToString();
                            break;
                        case "AL":
                            user.IsAutoLogin = (arrItem[1] == "0") ? false : true;
                            break;
                        case "UA":
                            user.UAccountNo = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "CA":
                            user.CAccountNo = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "DA":
                            user.DAccountNo = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "UT":
                            user.UserType = arrItem[1].ToString();
                            break;
                        case "SI":
                            user.UserSourceID = arrItem[1].ToString();
                            break;
                        case "DC":
                            user.DepLoginCount = arrItem[1].ToString();
                            break;
                        case "RI":
                            user.UserRoleId = arrItem[1].ToString();
                            break;
                    }
                }
            }
            user.UserRoleId = "1";
            return user;
        }

        public static UserCookieObject GetBusinessCookie(string userCookieValue)
        {
            if (string.IsNullOrEmpty(userCookieValue))
                return null;

            string cookieValue = UserCookieEncrypt.DecryptData(userCookieValue);
            if (string.IsNullOrEmpty(cookieValue))
                return null;
            var user = new UserCookieObject();
            string[] arrUserInfo = cookieValue.Split(';');
            foreach (string item in arrUserInfo)
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                string[] arrItem = item.Split('=');
                if (arrItem.Length == 2)
                {
                    switch (arrItem[0].ToUpper())
                    {
                        case "BUD":
                            user.BusinessId = Convert.ToInt64(arrItem[1]);
                            break;
                        case "NM":
                            user.UserName = UserCookieEncrypt.CookieDecode(arrItem[1]);
                            break;
                        case "CD":
                            user.CompanyId = Convert.ToInt64(String.IsNullOrEmpty(arrItem[1]) ? "0" : arrItem[1]);
                            break;
                        case "RD":
                            user.UserRoleId = String.IsNullOrEmpty(arrItem[1]) ? "0" : arrItem[1];
                            break;
                    }
                }
            }
            return user;
        }

        public static UserCookieObject GetUserInfo()
        {
            string strUserCookies = "";
            CookieManager cookieMng = new CookieManager();
            strUserCookies = cookieMng.getCookieValue("JSsUserInfo");
            if (string.IsNullOrEmpty(strUserCookies))
            {
                strUserCookies = cookieMng.getCookieValue("JSpUserInfo");
            }

            var user = GetUserInfoByCookie(strUserCookies);
            if (user == null)
                return null;
            string businessCookies = cookieMng.getCookieValue("businessInfo");
            if (!String.IsNullOrEmpty(businessCookies))
            {
                var businessUser = GetBusinessCookie(businessCookies);
                user.BusinessId = businessUser.BusinessId;
                user.UserName = businessUser.UserName;
                user.CompanyId = businessUser.CompanyId;
                user.UserRoleId = businessUser.UserRoleId;
            }
            return user;
        }
    }
}
