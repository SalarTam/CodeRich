using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code.Common.Cookie
{
    [Serializable]
    public class UserCookieObject
    {
        /// <summary>
        /// 用户COOKIE中的 UD
        /// 用户编号
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// 用户COOKIE中的 LN
        /// 用户登录名称
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 用户COOKIE中的 UN
        /// 用户姓名 
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 用户COOKIE中的 NN
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 用户COOIE中的 BD
        /// 登录公司ID
        /// </summary>
        public string LoginBUID { get; set; }

        /// <summary>
        /// 用户COOIE中的 BN
        /// 登录公司名称
        /// </summary>
        public string LoginBUName { get; set; }

        /// <summary>
        /// 用户COOIE中的 TM
        /// 用户权限串
        /// </summary>
        public string TokenMask { get; set; }

        /// <summary>
        /// 用户COOIE中的 EM
        /// 用户Email地址
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 用户COOIE中的 ST
        /// 用户状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 用户COOIE中的 AT
        /// 
        /// </summary>
        public string ActionTime { get; set; }

        /// <summary>
        /// 用户COOIE中的 DD
        /// 登录点ID
        /// </summary>
        public string DepartmentID { get; set; }

        /// <summary>
        /// 用户COOIE中的 DN
        /// 登录点名称
        /// </summary>
        public string DepartmentName { get; set; }

        /// <summary>
        /// 用户COOIE中的 LM
        /// 登录点LevelMark
        /// </summary>
        public string LevelMark { get; set; }

        /// <summary>
        /// 用户COOIE中的 CD
        /// 登录点所在城市 
        /// </summary>
        public string CityID { get; set; }

        /// <summary>
        /// 用户COOIE中的 AL
        /// 是否通过Email或者Cookie自动登录
        /// </summary>
        public bool IsAutoLogin { get; set; }

        /// <summary>
        /// 用户COOIE中的 UA
        /// 用户Account No
        /// </summary>
        public string UAccountNo { get; set; }

        /// <summary>
        /// 用户COOIE中的 CA
        /// 公司Account No
        /// </summary>
        public string CAccountNo { get; set; }

        /// <summary>
        /// 用户COOIE中的 DA
        /// 登录点Account No
        /// </summary>
        public string DAccountNo { get; set; }

        /// <summary>
        /// 用户COOIE中的 UT
        /// 用户类别
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// 用户COOIE中的 DC
        /// </summary>
        public string DepLoginCount { get; set; }

        /// <summary>
        /// 用户COOIE中的 SI
        /// </summary>
        public string UserSourceID { get; set; }
        /// <summary>
        /// 用户COOIE中的RI
        /// </summary>
        public string UserRoleId { get; set; }
       // public Role UserRole { get; set; }
        /// <summary>
        /// B端账号
        /// </summary>
        public long BusinessId { get; set; }
        public long CompanyId { get; set; }

    }
}
