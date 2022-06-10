using GameServer.Database;
using MyServer;
using Org.BouncyCastle.Security;
using Protocol.Code;
using Protocol.Dto;

namespace GameServer.Logic
{
    public class AccountHandler : IHandler
    {
        public void Disconnect(ClientPeer client)
        {
            DatabaseManager.OffLine(client);
        }

        public void Receive(ClientPeer client, int subCode, object value)
        {
            switch (subCode)
            {
                case AccountCode.Register_CREQ:
                    Register(client, value as AccountDto);
                    break;
                case AccountCode.Login_CREQ:
                    Login(client,value as AccountDto);
                    break;
                case AccountCode.GetUserInfo_CREQ:
                    GetUserInfo(client);
                    break;
                case AccountCode.GetRankList_CREQ:
                    GetRankList(client);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 客户端获取排行榜
        /// </summary>
        /// <param name="client"></param>
        private void GetRankList(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                RankListDto dto = DatabaseManager.GetRankListDto();
                client.SendMsg(OpCode.Account, AccountCode.GetRankList_SRES,dto);
            });
        }
        private void GetUserInfo(ClientPeer client)
        {
            SingleExecute.Instance.Execute(() =>
            {
                UserDto dto = DatabaseManager.CreateUserDto(client.Id);
                client.SendMsg(OpCode.Account, AccountCode.GetUserInfo_SRES,dto);
            });
        }
        private void Login(ClientPeer client, AccountDto dto)
        {
            SingleExecute.Instance.Execute(() =>
            {
                //用户不存在
                if (DatabaseManager.IsExistUserName(dto.userName) == false)
                {
                    client.SendMsg(OpCode.Account, AccountCode.Login_SRES, -1);
                    return;
                }
                // 密码不正确
                if (DatabaseManager.IsMatch(dto.userName,dto.passWord) == false)
                {
                    client.SendMsg(OpCode.Account, AccountCode.Login_SRES, -2);
                    return;
                }
                // 账号已在线
                if (DatabaseManager.IsOnline(dto.userName))
                {
                    client.SendMsg(OpCode.Account, AccountCode.Login_SRES, -3);
                    return;
                }
                DatabaseManager.Login(dto.userName,client);
                // 登录成功
                client.SendMsg(OpCode.Account, AccountCode.Login_SRES, 0);

            });
        }
        /// <summary>
        /// 客户端注册的处理
        /// </summary>
        /// <param name="dto"></param>
        private void Register(ClientPeer client, AccountDto dto)
        {
            SingleExecute.Instance.Execute(() =>
            {
                if (DatabaseManager.IsExistUserName(dto.userName))
                {
                    client.SendMsg(OpCode.Account, AccountCode.Register_SRES, -1);
                    return;
                }
                DatabaseManager.CreateUser(dto.userName,dto.passWord);
                client.SendMsg(OpCode.Account, AccountCode.Register_SRES, 0);
            });

        }
    }
}