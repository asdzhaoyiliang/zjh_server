using System;
using System.Collections.Generic;
using MyServer;
using MySql.Data.MySqlClient;
using Protocol.Dto;

namespace GameServer.Database
{
    public class DatabaseManager
    {
        private static MySqlConnection sqlConnect;
        private static Dictionary<int, ClientPeer> idClientDic;
        private static RankListDto rankListDto;

        public static void StartConnect()
        {
            rankListDto = new RankListDto();
            idClientDic = new Dictionary<int, ClientPeer>();
            string conStr = "database=zjhgame;data source=127.0.0.1;port=3306;user=root;pwd=root";
            sqlConnect = new MySqlConnection(conStr);
            sqlConnect.Open();
        }

        /// <summary>
        /// 判断是否存在该用户
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsExistUserName(string userName)
        {
            MySqlCommand cmd = new MySqlCommand("select UserName from userinfo where UserName = @userName", sqlConnect);
            cmd.Parameters.AddWithValue("userName", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool result = reader.HasRows;
            reader.Close();
            return result;
        }

        /// <summary>
        /// 创建用户信息
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        public static void CreateUser(string userName, string pwd)
        {
            MySqlCommand cmd =
                new MySqlCommand(
                    "insert into userinfo set UserName = @userName,Password = @pwd,Online=0,IconName = @iconName",
                    sqlConnect);
            cmd.Parameters.AddWithValue("userName", userName);
            cmd.Parameters.AddWithValue("pwd", pwd);
            int index = new Random().Next(0, 19);
            cmd.Parameters.AddWithValue("iconName", "headIcon_" + index.ToString());
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 判断密码是否正确
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static bool IsMatch(string userName, string pwd)
        {
            MySqlCommand cmd = new MySqlCommand("select * from userinfo where UserName = @userName", sqlConnect);
            cmd.Parameters.AddWithValue("userName", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool result = false;
            if (reader.HasRows)
            {
                reader.Read();
                result = reader.GetString("PassWord") == pwd;
            }

            reader.Close();
            return result;
        }

        /// <summary>
        /// 判断用户是否在线
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static bool IsOnline(string userName)
        {
            MySqlCommand cmd = new MySqlCommand("select Online from userinfo where UserName = @userName", sqlConnect);
            cmd.Parameters.AddWithValue("userName", userName);
            MySqlDataReader reader = cmd.ExecuteReader();
            bool result = false;
            if (reader.HasRows)
            {
                reader.Read();
                result = reader.GetBoolean("Online");
            }

            reader.Close();
            return result;
        }

        /// <summary>
        /// 登录上线
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public static void Login(string userName, ClientPeer client)
        {
            MySqlCommand cmd =
                new MySqlCommand("update userinfo set Online = 1 where UserName = @userName", sqlConnect);
            cmd.Parameters.AddWithValue("userName", userName);
            cmd.ExecuteNonQuery();

            MySqlCommand cmd1 = new MySqlCommand("select * from userinfo where UserName = @userName", sqlConnect);
            cmd1.Parameters.AddWithValue("userName", userName);
            MySqlDataReader reader = cmd1.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                int id = reader.GetInt32("Id");
                client.Id = id;
                client.UserName = userName;
                if (idClientDic.ContainsKey(id) == false)
                {
                    idClientDic.Add(id, client);
                }
            }

            reader.Close();
        }

        /// <summary>
        /// 用户下线
        /// </summary>
        /// <param name="client"></param>
        public static void OffLine(ClientPeer client)
        {
            if (idClientDic.ContainsKey(client.Id))
            {
                idClientDic.Remove(client.Id);
            }

            MySqlCommand cmd = new MySqlCommand("update userinfo set Online = 0 where Id = @id", sqlConnect);
            cmd.Parameters.AddWithValue("id", client.Id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// 使用用户ID获取客户端连接对象
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static ClientPeer GetClientPeerByUserId(int userId)
        {
            if (idClientDic.ContainsKey(userId))
            {
                return idClientDic[userId];
            }

            return null;
        }

        /// <summary>
        /// 构造用户信息模型
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static UserDto CreateUserDto(int userId)
        {
            MySqlCommand cmd = new MySqlCommand("select * from userinfo where Id = @id", sqlConnect);
            cmd.Parameters.AddWithValue("id", userId);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                string userName = reader.GetString("UserName");
                string iconName = reader.GetString("IconName");
                int coinCount = reader.GetInt32("Coin");
                UserDto dto = new UserDto(userId, userName, iconName, coinCount);
                reader.Close();
                return dto;
            }

            reader.Close();
            return null;
        }

        public static RankListDto GetRankListDto()
        {
            MySqlCommand cmd = new MySqlCommand("select UserName,Coin from userinfo order By Coin desc",sqlConnect);
            MySqlDataReader reader = cmd.ExecuteReader();
            rankListDto.Clean();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    RankItemDto dto = new RankItemDto(reader.GetString("UserName"), reader.GetInt32("Coin"));
                    rankListDto.Add(dto);
                }
            }
            reader.Close();
            return rankListDto;
        }

        public static int UpdateCoin(ClientPeer client, int coinCount)
        {
            MySqlCommand cmd = new MySqlCommand("update userinfo set Coin = Coin + @coinCount where Id = @id", sqlConnect);
            cmd.Parameters.AddWithValue("coinCount", coinCount);
            cmd.Parameters.AddWithValue("id", client.Id);
            cmd.ExecuteNonQuery();

            MySqlCommand cmd1 = new MySqlCommand("select Coin from userinfo where Id = @id", sqlConnect);
            cmd1.Parameters.AddWithValue("id", client.Id);
            MySqlDataReader reader = cmd1.ExecuteReader();
            int res = 0;
            if (reader.HasRows)
            {
                reader.Read();
                res = reader.GetInt32("Coin");
            }
            reader.Close();
            return res;
        }
    }
}