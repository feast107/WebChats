using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using MonitorHub.Models;
using System.Security.Claims;
using System.Threading.Channels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using MonitorHub.Events;

namespace MonitorHub.Hubs
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MonitorHub : Hub 
    {

        #region 全局
        private const string AuthSchemes =  JwtBearerDefaults.AuthenticationScheme;
        private readonly static Dictionary<string, HubUser> users = new ();
        private readonly static List<HubGroup> groups = new List<HubGroup>();
        #endregion

        #region 私有方法

      
        private static string JSON(object o)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(o);
        }
        private static T? DESJ<T>(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }

        private string GetUserId()
        {
            return GetValue(JwtRegisteredClaimNames.Sid);
        }
        private string GetUserName()
        {
            return GetValue(ClaimTypes.Name);
        }
        private string GetValue(string ClaimType)
        {
            var Claim = GetClaims()?.Find(x => x.Type == ClaimType);
            return Claim?.Value ?? "";
        }
        private List<Claim>? GetClaims()
        {
            return Context.User?.Claims.ToList();
        }
        private HubUser MyHub()
        {
            if (users.TryGetValue(GetUserId(), out HubUser user))
            {
                return user;
            }
            else
            {
                var newuser = new HubUser(true) 
                { 
                    UserId = GetUserId(),
                    UserName = GetUserName(), 
                    ConnectionId = Context.ConnectionId, 
                    HubGroups = new List<HubGroup>() 
                };
                users.Add(GetUserId(), newuser);
                return newuser;
            }
        }


        private void AddAllToGroup(HubGroup group)
        {
            List<Task> tasks = new List<Task>();
            users.Values.ToList().ForEach(x =>
            {
                var t = Groups.AddToGroupAsync(x.ConnectionId, group.GroupId);
                tasks.Add(t);
            });
            Task.WaitAll(tasks.ToArray());
        }
        private void JoinInAllGroup(HubUser user)
        {
            List<Task> tasks = new List<Task>();
            groups.ToList().ForEach(x =>
            {
                var t = Groups.AddToGroupAsync(user.ConnectionId, x.GroupId);
                tasks.Add(t);
            });
            Task.WaitAll(tasks.ToArray());
        }
        #endregion

        #region 枚举
        public enum Methods
        {
            System,
            Entire,
            Group,
            Client
        }

        public enum Handlers
        {
            Alert,
            Init,
            Groups,
            Clients,
            OffLine,
            OnLine,
            GroupCreate,
            GroupMessage,
            ClientMessage
        }
        #endregion

        public MonitorHub()
        {
        }

        #region 连接
        public override Task OnConnectedAsync()
        {
            Console.WriteLine(Context.ConnectionId+"连接");
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine(Context.ConnectionId + "断开");
            BroadcastOthers(nameof(Handlers.OffLine),JSON(MyHub())).Wait();
            users.Remove(GetUserId());
            return base.OnDisconnectedAsync(exception);
        }
        #endregion

        public async Task JoinGroup(string GroupId,object Candidate)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupId);
            await Clients.Caller.SendAsync(nameof(Methods.System),nameof(JoinGroup), GroupId, Candidate);
        }

        public async Task LeaveGroup(string GroupId,object Candidate)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupId);
            await Clients.Caller.SendAsync(nameof(Methods.System),nameof(LeaveGroup), GroupId, Candidate);
        }

        public async Task InviteToGroup(string ConnectionId,string GroupId,object Invitor)
        {
            await Clients.Client(ConnectionId).SendAsync(nameof(Methods.System),nameof(InviteToGroup), GroupId, Invitor);
        }

        #region 接收
        public async Task Init()
        {
            await UnicastSelf(nameof(Handlers.Clients), JSON(users.Values.ToList()));
            var myHub = MyHub();
            JoinInAllGroup(myHub);
            await UnicastSelf(nameof(Handlers.Init),JSON(myHub));
            await BroadcastOthers(nameof(Handlers.OnLine), JSON(myHub));
            await UnicastSelf(nameof(Handlers.Groups), JSON(groups));
        }
        public async Task CreateGroup(string GroupName)
        {
            var hub = MyHub();
            if (hub.Own != null)
            {
                await UnicastSelf(nameof(Handlers.Alert), "您正管理着一个组");
            }
            else
            {
                HubGroup group = new()
                {
                    Clients = new(),
                    Creator = hub,
                    GroupId = Guid.NewGuid().ToString(),
                    GroupName = GroupName
                };
                hub.Own = group;
                hub.HubGroups.Add(group);
                groups.Add(group);
                AddAllToGroup(group);
                await Broadcast(nameof(Handlers.GroupCreate), JSON(group));
            }
        }

        public async Task GroupMessage(Event<HubUser,HubGroup, HubMessage> e)
        {
            HubGroup group = e.P;
            await MulticastOthers(group.GroupId, nameof(Handlers.GroupMessage), JSON(e));
        }

        public async Task ClientMessage(Event<HubUser,HubUser,HubMessage> e)
        {
            HubUser user = e.P;
            await Unicast(user.ConnectionId, nameof(Handlers.ClientMessage), JSON(e));
        }
        #endregion

        #region 发送
        #region 广播
        public async Task Broadcast(string handler,object JSON)
        {
            await Clients.All.SendAsync(nameof(Methods.Entire), handler,JSON);
        }

        public async Task BroadcastOthers(string handler, object JSON)
        {
            await Clients.Others.SendAsync(nameof(Methods.Entire), handler, JSON);
        }
        #endregion

        #region 组播
        public async Task Multicast(string handler, object JSON)
        {
            await Clients.Groups(MyHub().Groups).SendAsync(nameof(Methods.Group), handler, JSON);
        }
        public async Task MulticastGroup(string GroupId, string handler, object JSON)
        {
            await Clients.Group(GroupId).SendAsync(nameof(Methods.Group), handler, JSON);
        }
        public async Task MulticastGroups(string[] GroupIds, string handler, object JSON)
        {
            await Clients.Groups(GroupIds).SendAsync(nameof(Methods.Group), handler, JSON);
        }
        public async Task MulticastOthers(string GroupId, string handler, object JSON)
        {
            await Clients.OthersInGroup(GroupId).SendAsync(nameof(Methods.Group), handler, JSON);
        }

        #endregion

        #region 单播
        public async Task Unicast(string ConnectionId,string handler, object JSON)
        {
            await Clients.Client(ConnectionId).SendAsync(nameof(Methods.Client), handler, JSON);
        }
        public async Task UnicastSelf(string handler, object JSON)
        {
            await Clients.Caller.SendAsync(nameof(Methods.System), handler, JSON);
        }
        #endregion
        #endregion

        #region 弃用
        [Obsolete]
        public ChannelReader<int> Counter(int count,int delay,CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<int>();

            // We don't want to await WriteItemsAsync, otherwise we'd end up waiting 
            // for all the items to be written before returning the channel back to
            // the client.
            _ = WriteItemsAsync(channel.Writer, count, delay, cancellationToken);

            return channel.Reader;
        }

        [Obsolete]
        public ChannelReader<int> Count()
        {
            var channel = Channel.CreateUnbounded<int>();

            // We don't want to await WriteItemsAsync, otherwise we'd end up waiting 
            // for all the items to be written before returning the channel back to
            // the client.
            _ = WriteItemsAsync(channel.Writer);

            return channel.Reader;
        }
        [Obsolete]
        private async Task WriteItemsAsync(ChannelWriter<int> writer)
        {
            Exception localException;
            try
            {
                for (var i = 0; i < 10; i++)
                {
                    await writer.WriteAsync(i);

                    // Use the cancellationToken in other APIs that accept cancellation
                    // tokens so the cancellation can flow down to them.
                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                localException = ex;
            }
            finally
            {
                writer.Complete();
            }
        }
        [Obsolete]
        private async Task WriteItemsAsync( ChannelWriter<int> writer, int count,int delay,CancellationToken cancellationToken)
        {
            Exception localException = null;
            try
            {
                for (var i = 0; i < count; i++)
                {
                    await writer.WriteAsync(i, cancellationToken);

                    // Use the cancellationToken in other APIs that accept cancellation
                    // tokens so the cancellation can flow down to them.
                    await Task.Delay(delay, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                localException = ex;
            }
            finally
            {
                writer.Complete(localException);
            }
        }
        [Obsolete]
        public async Task UploadStream(ChannelReader<string> stream)
        {
            while (await stream.WaitToReadAsync())
            {
                while (stream.TryRead(out var item))
                {
                    // do something with the stream item
                    Console.WriteLine(item);
                }
            }
        }
        #endregion

    }
}
