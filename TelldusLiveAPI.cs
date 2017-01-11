using System;
using System.Collections.Generic;
using RestSharp;
using RestSharp.Authenticators;
using RestSharp.Deserializers;

namespace TelldusLiveAPI
{
    public enum DelayPolicy { Restart, Continue };
    public enum Days { Monday=1, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday };
    public enum JobType { Time, Sunrise, Sunset };
    public enum ClientExtra { Coordinate, Features, Latestversion, Suntime, Timezone, Transports, Tzoffset }
    public enum DeviceExtra { Coordinate, Timezone, Transport, Tzoffset }
    public enum ValueType { Temp, Humidity, Wgust, Wavg, Watt, Ill, Rrate, Unknown }
    public enum Command
    {
        TURNON = 1,
        TURNOFF = 2,
        BELL = 4,
        TOGGLE = 8,
        DIM = 16,
        LEARN = 32,
        EXECUTE = 64,
        UP = 128,
        DOWN = 256,
        STOP = 512,
    }
    class TelldusLive
    {
        
        private string _Publickey;
        private string _PrivateKey;

        private RestClient client = new RestClient("https://api.telldus.com");
        
        public TelldusLive()
        {

        }

        public TelldusLive(string Publickey, string PrivateKey)
        {
            _Publickey = Publickey;
            _PrivateKey = PrivateKey;
        }

        public Result Login(string username, string password) // Need a new API key to test this
        {           
            client.Authenticator = OAuth1Authenticator.ForClientAuthentication(_Publickey, _PrivateKey, username, password);
            var request = new RestRequest("json/user/profile", Method.GET);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }
        public Result Login(string PublicKey, string PrivateKey,string Token, string TokenSecret)
        {
            client.Authenticator = OAuth1Authenticator.ForProtectedResource(PublicKey, PrivateKey, Token, TokenSecret);
            var request = new RestRequest("json/user/profile", Method.GET);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        //Clients
        /// <summary>
        /// Returns a list of all clients associated with the current user
        /// </summary>
        /// <param name="extras">(optional) A comma-delimited list of extra information to fetch for each returned client. Currently supported fields are: coordinate, features, latestversion, suntime, timezone, transports and tzoffset</param>
        /// <returns>a list off Clients data</returns>
        public List<Client> ClientsList(List<ClientExtra> extras = null)
        {
            string stringExtras = "";
            var request = new RestRequest("xml/clients/list", Method.GET);
            if (extras != null)
            {
                foreach(ClientExtra item in extras)
                {
                    stringExtras += item.ToString().ToLower() + ",";
                }
                request.AddParameter("extras", stringExtras.TrimEnd(','));
            }
            IRestResponse<List<Client>> response = client.Execute<List<Client>>(request);
            return response.Data;
        }
        
        //Client

        /// <summary> 
        ///  Gets info about a specifik client
        /// </summary>          
        ///<param name="id">The id of the client</param>
        /// <param name="uuid">(optional) An optional uuid for a client. By specifying the uuid, info about a non registered client can be fetched</param>  
        /// <param name="code">(optional) If a activation code from a TellStick Net is supplied here the uuid could be omitted. Only non activated units can be fetched this way.</param>  
        /// <param name="extras">(optional) A comma-delimited list of extra information to fetch for each returned client. Currently supported fields are: coordinate, suntime, timezone and tzoffset</param>
        /// <returns>a Clinent data</returns>  
        public Client ClientInfo(string id, List<ClientExtra> extras = null, string uuid = null,
            string code = null) // TODO: fix so that varibule code works as intended
        {
            string stringExtras = "";
            var request = new RestRequest("xml/client/info", Method.GET);
            request.AddParameter("id", id);
            if (uuid != null)
            {
                request.AddParameter("uuid", uuid);
            }
            if (code != null)
            {
                request.AddParameter("code", code);
            }
            if (extras != null)
            {
                foreach (ClientExtra item in extras)
                {
                    stringExtras += item.ToString().ToLower() + ",";
                }
                request.AddParameter("extras", stringExtras.TrimEnd(','));
            }
            IRestResponse<Client> response = client.Execute<Client>(request);
            response.Data.Clean();
            return response.Data;
            
        }

        /// <summary>
        /// Register an unregistered client to the calling user
        /// </summary>
        /// <param name="id">This is an unique id for the client</param>
        /// <param name="uuid">	The specific clients uuid</param>
        /// <returns>returns true if client is registerd</returns>
        public ResultAdd ClientRegister(string id, string uuid)   //TODO: test this 
        {
            var request = new RestRequest("json/client/register", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("uuid", uuid);
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;

        }

        /// <summary>
        /// Removes a client from the user. The client needs to be activated again in order to be used 
        /// </summary>
        /// <param name="id">This is an unique id for the client</param>
        /// <returns>returns true if client is removed</returns>
        public Result ClientRemove(string id)  //todo: testa denna
        {
            var request = new RestRequest("json/client/remove", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Renames a client
        /// </summary>
        /// <param name="id">The id of the client</param>
        /// <param name="name">The new name</param>
        /// <returns>true if successful</returns>
        public Result ClientSetName(string id, string name)
        {
            var request = new RestRequest("json/client/setName", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("name", name);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Enables or disables push from this client. The current API key must be configured for push for this to work.
        /// </summary>
        /// <param name="id">The id of the client</param>
        /// <param name="enable">True enabling push, False disabling push</param>
        /// <returns></returns>
        public Result ClientSetPush(string id, bool enable) //Can't test this requiers a speccial API key.
        {
            var request = new RestRequest("json/client/setPush", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("enable", (enable) ? "1" : "0");
            IRestResponse<Result> response = client.Execute<Result>(request);
            return response.Data;
        }

        //Devices

        /// <summary>
        /// Returns a list of all devices associated with the current user
        /// </summary>
        /// <param name="includeIgnored">Set to 1 to include ignored devices</param>
        /// <param name="supportedMethods">The methods supported by the calling application. If this parameter is not set the methods and state will always report 0.</param>
        /// <param name="extras">(optional) A comma-delimited list of extra information to fetch for each returned device. Currently supported fields are: coordinate, timezone, transport, and tzoffset</param>
        /// <returns>List off Devices</returns>
        public List<Device> DevicesList(bool includeIgnored = false, 
            int supportedMethods = 0, List<DeviceExtra> extras = null) //TODO: testa att supportMethods fungerar
        {
            string stringExtras = "";
            var request = new RestRequest("xml/devices/list", Method.GET);
            request.AddParameter("includeIgnored", (includeIgnored) ? "1" : "0");
            request.AddParameter("supportedMethods", supportedMethods);
            if (extras != null)
            {
                foreach (ClientExtra item in extras)
                {
                    stringExtras += item.ToString().ToLower() + ",";
                }
                request.AddParameter("extras", stringExtras.TrimEnd(','));
            }
            IRestResponse<List<Device>> response = client.Execute<List<Device>>(request);
            return response.Data;
        }

        //Device
        /// <summary>
        /// Adds a new device and connects it to a client. The client must be editable for this to work
        /// </summary>
        /// <param name="clientId">The id of the client</param>
        /// <param name="name">The name of the device</param>
        /// <param name="protocol">The protocol</param>
        /// <param name="model">The model</param>
        /// <returns>Class with result and Error info</returns>
        public ResultAdd DeviceAdd(string clientId, string name,
            string protocol, string model)
        {
            var request = new RestRequest("json/device/add", Method.GET);
            request.AddParameter("clientId", clientId);
            request.AddParameter("name", name);
            request.AddParameter("protocol", protocol);
            request.AddParameter("model", model);
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        public Result DeviceBell(string id)//TODO: Missing a door bell to test this.
        {
            var request = new RestRequest("json/device/bell", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Sends a command to a device
        /// </summary>
        /// <param name="id">The id of the device to turn off</param>
        /// <param name="method">This should be any of the method constants</param>
        /// <param name="value">For command where a value is needed, this is the value</param>
        /// <returns>True on success</returns>
        public Result DeviceCommand(string id, Command method, string value)
        {
            var request = new RestRequest("json/device/command", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("method", method);
            request.AddParameter("value", value);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Sends a dim command to devices supporting this.
        /// </summary>
        /// <param name="id">The device id to dim.</param>
        /// <param name="level">The level the device should dim to. This value should be 0-255</param>
        /// <returns>True if message sent with out Error</returns>
        public Result DeviceDim(string id, byte level)
        {
            var request = new RestRequest("json/device/dim", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("level", level.ToString());
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        ///  Sends a dim command in procent to a devices that supporting this.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="levelProcent"></param>
        /// <returns></returns>
        public Result DeviceDimProcent(string id, byte levelProcent)
        {
            return DeviceDim(id, (byte)(levelProcent * 2.55));
        }

        public Result DeviceDown(string id)//TODO: Missing device to test this
        {
            var request = new RestRequest("json/device/down", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// BETA Parameters and data structure may be modified without notice BETA Get all device states from a certain period. The data is limited to 5000 rows. For some device types, failures will be logged too. If successStatus is anything else than 0, the command was NOT executed.
        /// </summary>
        /// <param name="id">The id of the device</param>
        /// <param name="to">Timestamp to, in seconds</param>
        /// <param name="from">Timestamp from, in seconds</param>        
        /// <returns>List off events with data</returns>
        public List<History> DeviceHistory(string id, long to, long from = 0)
        {
            var request = new RestRequest("xml/device/history", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("from", from.ToString());
            request.AddParameter("to", to.ToString());
            
            IRestResponse<List<History>> response = client.Execute<List<History>>(request);                 
            return response.Data;
        }

        /// <summary>
        /// BETA Parameters and data structure may be modified without notice BETA Get all device states from a certain period. The data is limited to 5000 rows. For some device types, failures will be logged too. If successStatus is anything else than 0, the command was NOT executed.
        /// </summary>
        /// <param name="id">The id of the device</param>
        /// <param name="to">Timestamp to, in seconds</param>
        /// <param name="from">Timestamp from, in seconds</param>
        /// <param name="extras">(optional) A comma-delimited list of extra information to fetch for each returned device. Currently supported fields are: timezone and tzoffset</param>
        /// <returns>List off events with data</returns>
        public ExtraHistory DeviceHistory(string id, long to, long from = 0,
            List<DeviceExtra> extras = null)
        {
            string stringExtras = "";
            var request = new RestRequest("json/device/history", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("from", from.ToString());
            request.AddParameter("to", to.ToString());
            if (extras != null)
            {
                foreach (DeviceExtra item in extras)
                {
                    stringExtras += item.ToString().ToLower() + ",";
                }
                request.AddParameter("extras", stringExtras.TrimEnd(','));
            }
            IRestResponse<ExtraHistory> response = client.Execute<ExtraHistory>(request);
            return response.Data;
        }

        /// <summary>
        /// Returns information about a specific device
        /// </summary>
        /// <param name="id">The id of the device</param>
        /// <param name="supportedMethods">The methods supported by the calling application. If this parameter is not set the methods and state will always report 0.</param>
        /// <param name="extras">(optional) A comma-delimited list of extra information to fetch for each returned device.Currently supported fields are: coordinate, timezone, transport, and tzoffset</param>
        /// <returns>a Device object</returns>
        public Device DeviceInfo(string id, int supportedMethods = 0, 
            List<DeviceExtra> extras = null)
        {
            string stringExtras = "";
            var request = new RestRequest("xml/device/info", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("supportedMethods", supportedMethods);
            if (extras != null)
            {
                foreach (DeviceExtra item in extras)
                {
                    stringExtras += item.ToString().ToLower() + ",";
                }
                request.AddParameter("extras", stringExtras.TrimEnd(','));
            }
            IRestResponse<Device> response = client.Execute<Device>(request);
            response.Data.Clean();
            return response.Data;
        }

        /// <summary>
        /// Sends a special learn command to some devices that need a special learn-command to be used from TellStick
        /// </summary>
        /// <param name="id">The device id to learn.</param>
        /// <returns>True if message sent without Error</returns>
        public Result DeviceLearn(string id)
        {
            var request = new RestRequest("json/device/learn", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Removes a device. It is only possible to remove editable devices.
        /// </summary>
        /// <param name="id">The id of the device</param>
        /// <returns>Class with result and error info</returns>
        public Result DeviceRemove(string id)
        {
            var request = new RestRequest("json/device/remove", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Mark a device as 'ignored'. Ignored devices will not show up in devices/list if not explicit set to do so.
        /// </summary>
        /// <param name="id">The id of the device</param>
        /// <param name="ignore">true to ignor and false for NOT ignored</param>
        /// <returns>true for success</returns>
        public Result DeviceSetIgnore(string id, bool ignore)
        {
            var request = new RestRequest("json/device/setIgnore", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("ignore", (ignore) ? "1" : "0");
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Renames a device
        /// </summary>
        /// <param name="id">The id of the device</param>
        /// <param name="name">The new name</param>
        /// <returns>True if message sent without Error</returns>
        public Result DeviceSetName(string id, string name)
        {
            var request = new RestRequest("json/device/setName", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("name", name);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Set device model
        /// </summary>
        /// <param name="id">The id of the device</param>
        /// <param name="model">	The new model</param>
        /// <returns>True if message sent without Error</returns>
        public Result DeviceSetModel(string id, string model)
        {
            var request = new RestRequest("json/device/setModel", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("model", model);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Set device protocol
        /// </summary>
        /// <param name="id">The id of the device</param>
        /// <param name="protocol">The new protocol name</param>
        /// <returns>True if message sent without Error</returns>
        public Result DeviceSetProtocol(string id, string protocol)
        {
            var request = new RestRequest("json/device/setProtocol", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("protocol", protocol);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        public Result DeviceSetParameter(string id, string parameter, string value)//TODO: Test this
        {
            var request = new RestRequest("json/device/setParameter", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("parameter", parameter);
            request.AddParameter("value", value);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Send a "stop" command to a device that is scaning for remote control signals.
        /// </summary>
        /// <param name="id">The device id to stop.</param>
        /// <returns>True if message sent with out error</returns>
        public Result DeviceStop(string id)//TODO: Test this
        {
            var request = new RestRequest("json/device/stop", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Turns a device off.
        /// </summary>
        /// <param name="id">The device id to turn off.</param>
        /// <returns>True if message sent without error</returns>
        public Result DeviceTurnOff(string id)
        {
            var request = new RestRequest("json/device/turnOff", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Turns a device on.
        /// </summary>
        /// <param name="id">The device id to turn on.</param>
        /// <returns>True if message sent with out error</returns>
        public Result DeviceTurnOn(string id)
        {
            var request = new RestRequest("json/device/turnOn", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        public Result DeviceUp(string id)//TODO: Missing device to test this.
        {
            var request = new RestRequest("json/device/up", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        //Events
        /// <summary>
        /// Returns a list of events
        /// </summary>
        /// <param name="listOnly">Only list the events and do not return triggers, conditions and actions. This must be set to True. This is only for backwards compatibility with older applications.</param>
        /// <returns>Class with Events</returns>
        public List<Event> EventsList(bool listOnly = false)
        {
            const string startToken = "<event ";
            const string stopToken = "</event>";
            XmlDeserializer d = new XmlDeserializer();
            int begining = 0;
            int end = 0;

            var request = new RestRequest("xml/events/list", Method.GET);
            request.AddParameter("listOnly", (listOnly)?"1":"0");
            IRestResponse<List<Event>> response = client.Execute<List<Event>>(request);
            string content = response.Content;

            if (!listOnly)
            {
                for (int i = 0; i < response.Data.Count; i++)
                {
                    begining = response.Content.IndexOf(startToken, end);
                    end = response.Content.IndexOf(stopToken, begining) + (stopToken.Length);
                    response.Content = response.Content.Substring(begining, (end - begining));
                    response.Data[i].Trigger = d.Deserialize<List<Trigger>>(response);
                    response.Data[i].Condition = d.Deserialize<List<Condition>>(response);
                    response.Data[i].Action = d.Deserialize<List<Action>>(response);
                    response.Content = content;
                }
            }

            return response.Data;                           
        }

        //Event
        /// <summary>
        /// Returns the info of an event.
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <returns>a Event class object</returns>
        public Event EventInfo(string id)
        {
            var request = new RestRequest("json/event/info", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Event> response = client.Execute<Event>(request);
            return response.Data;
        }

        /// <summary>
        /// Adds or updates a event to the system. Set id to 0 if you want to create a new event
        /// </summary>
        /// <param name="description">A user friendly description for this event</param>
        /// <param name="active">True if active and false id inactive</param>
        /// <param name="id">The id of the event if not set then one is generated.</param>
        /// <param name="minRepeatInterval">(needs pro)Sets the minimum time that needs to pass before this event can execute again. Defaults to 30 seconds.</param>
        /// <returns></returns>
        public ResultAdd EventSetEvent(string description, bool active = true,
            string id = null, int ?minRepeatInterval=null)
        {
            var request = new RestRequest("json/event/setEvent", Method.GET);
            if (id != null)
            {
                request.AddParameter("id", id);
            }
            if (minRepeatInterval != null)
            {
                request.AddParameter("minRepeatInterval", minRepeatInterval.ToString());  // need pro for this 
            }
            request.AddParameter("description", description);
            request.AddParameter("active", (active) ? "1" : "0");
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        public ResultAdd EventSetBlockHeaterTrigger(string eventId, string sensorId,
            byte hour, byte minute, string id=null)//TODO: Need pro.
        {
            var request = new RestRequest("json/event/setBlockHeaterTrigger", Method.GET);
            if(id != null)
            {
                request.AddParameter("id", id);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("sensorId", sensorId);
            request.AddParameter("hour", hour.ToString());
            request.AddParameter("minute", minute.ToString());
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update a device as an action to an event.
        /// </summary>
        /// <param name="eventId">The id of the event to add the action to</param>
        /// <param name="deviceId">The id of the device to command</param>
        /// <param name="method">The method to execute</param>
        /// <param name="value">For command where a value is needed, this is the value</param>
        /// <param name="repeats">Number of times (1-10) the command should be repeated (with a 3s delay between)</param>
        /// <param name="delayPolicy">If this action is activated a second time while waiting on the delay this sets the policy. It could be one of:restart: Restarts the timer continue: The second activation has no effect.The first timer continues to run This have no effect if no delay is set.</param>
        /// <param name="id">The id of the action. Leave empty to create a new action</param>
        /// <param name="delay">Number of seconds delay before executing this action. Setting this requires Pro</param>
        /// <returns>a string with the new id that has been generated</returns>
        public ResultAdd EventSetDeviceAction(string eventId, string deviceId,
            byte method, byte value, byte repeats, DelayPolicy delayPolicy,
            string id = null, byte? delay = null)
        {
            var request = new RestRequest("json/event/setDeviceAction", Method.GET);
            if (id != null)
            {
                request.AddParameter("id", id);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("deviceId", deviceId);
            request.AddParameter("method", method);
            request.AddParameter("value", value);
            request.AddParameter("repeats", repeats.ToString());
            request.AddParameter("delayPolicy", delayPolicy.ToString().ToLower());
            if (delay != null)  // requiers pro 
            {
                request.AddParameter("delay", delay.ToString());
            }
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if(response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update a device as condition to an event.
        /// </summary>
        /// <param name="eventId">The id of the event to add the trigger to</param>
        /// <param name="deviceId">The id of the device to query</param>
        /// <param name="method">The state the device must be in</param>
        /// <param name="group">If group is set you get an AND condition if not you get an ELES condition</param>
        /// <param name="id">The id of the condition. Leave empty to create a new condition</param>
        /// <returns>Class contianing result id, group and error info</returns>
        public ResultAdd EventSetDeviceCondition(string eventId, string deviceId, 
            int method = 0, string group=null, string id = null)
        {
            var request = new RestRequest("json/event/setDeviceCondition", Method.GET);
            if (id != null)
            {
                request.AddParameter("id", id);
            }
            if(group != null)
            {
                request.AddParameter("group", group);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("deviceId", deviceId);
            request.AddParameter("method", method);
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update a device as trigger to an event.
        /// </summary>
        /// <param name="eventId">The id of the event to add the trigger to</param>
        /// <param name="deviceId">The id of the device to monitor</param>
        /// <param name="method">The value to trigger on</param>
        /// <param name="id">The id of the trigger. Leave empty to create a new trigger</param>
        /// <returns>Class with result and errorinfo</returns>
        public ResultAdd EventSetDeviceTrigger(string eventId, string deviceId,
            byte method, string id = null)
        {
            var request = new RestRequest("json/event/setDeviceTrigger", Method.GET);
            if (id != null)
            {
                request.AddParameter("id", id);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("deviceId", deviceId);
            request.AddParameter("method", method.ToString());
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        public ResultAdd EventSetEmailAction(string eventId, string address, string message, 
            DelayPolicy delayPolicy, string id=null, byte ?delay=null)//TODO:Need pro. to test this
        {
            // <returns>True if message sent without Error</returns>
            var request = new RestRequest("json/event/setEmailAction", Method.GET);
            if( id != null)
            {
                request.AddParameter("id", id);
            }
            if( delay != null)  // requiers pro account
            {
                request.AddParameter("delay", delay.ToString());
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("address", address);
            request.AddParameter("message", message);
            request.AddParameter("delayPolicy", delayPolicy);
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update a new push-to-phone-action to an event.
        /// </summary>
        /// <param name="eventId">The id of the event to add the action to</param>
        /// <param name="phoneId">The id of the phone to send the push notification to</param>
        /// <param name="message">The message to send</param>
        /// <param name="delayPolicy">	If this action is activated a second time while waiting on the delay this sets the policy. It could be one of:restart: Restarts the timer continue: The second activation has no effect.The first timer continues to run This have no effect if no delay is set.</param>
        /// <param name="id">The id of the action. Leave empty to create a new action</param>
        /// <param name="delay">Number of seconds delay before executing this action. Setting this requires Pro</param>
        /// <returns>Class contianing result id and error info</returns>
        public ResultAdd EventSetPushAction(string eventId, string phoneId, string message, 
            DelayPolicy delayPolicy, string id=null, byte ?delay=null)
        {
            var request = new RestRequest("json/event/setPushAction", Method.GET);
            if (id != null)
            {
                request.AddParameter("id", id);
            }
            if (delay != null)  // requiers pro account
            {
                request.AddParameter("delay", delay.ToString());
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("phoneId", phoneId);
            request.AddParameter("message", message);
            request.AddParameter("delayPolicy", delayPolicy.ToString().ToLower());
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if( response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        public ResultAdd EventSetSMSAction(string eventId, string to, string message, bool flash, 
            DelayPolicy delayPolicy, string id=null, byte ?delay=null)//TODO:Need pro. to test this
        {
            var request = new RestRequest("json/event/setSMSAction", Method.GET);
            if (id != null)
            {
                request.AddParameter("id", id);
            }
            if (delay != null)  // requiers pro account
            {
                request.AddParameter("delay", delay.ToString());
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("to", to);
            request.AddParameter("message", message);
            request.AddParameter("flash", flash);
            request.AddParameter("delay", delay.ToString());
            request.AddParameter("delayPolicy", delayPolicy.ToString().ToLower());
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update a sensor as condition to an event.
        /// </summary>
        /// <param name="eventId">The id of the event to add the condition to</param>
        /// <param name="group">	The condition group to add this condition to. All conditions in a group must be true for the action to happen. If this is not set or null a new group will be created.</param>
        /// <param name="sensorId">The id of the sensor to query</param>
        /// <param name="value">The value to trigger on</param>
        /// <param name="edge">Rising or falling edge? Accepted values are: True: Rising edge.When the new value bigger then old value Null: Equal.When new value == old value False: Falling edge.When new value less then old value</param>
        /// <param name="valueType">Value from enum ValueTyp (The type of the value. Accepted values are: temp, humidity, wgust (wind gust), wavg (wind average), watt (power), ill (illumination), rrate (rain rate) and unknown)</param>
        /// <param name="scale">(optional) The value scale (unit type) used.</param>
        /// <param name="id">The id of the condition. Leave empty to create a new condition</param>
        /// <returns>Class with result and errorinfo</returns>
        public ResultAdd EventSetSensorCondition(string eventId, string group, string sensorId,
            int value, bool ?edge, ValueType valueType, string scale=null, string id=null)
        {
            var request = new RestRequest("json/event/setSensorCondition", Method.GET);
            if( id != null)
            {
                request.AddParameter("id", id);
            }
            if( scale != null)
            {
             request.AddParameter("scale", scale);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("group", group);
            request.AddParameter("sensorId", sensorId);
            request.AddParameter("value", value.ToString());
            if (edge == true)
            {
                request.AddParameter("edge", "1");
            }
            else if (edge == false)
            {
                request.AddParameter("edge", "-1");
            }
            else if (edge == null)
            {
                request.AddParameter("edge", "0");
            }
            request.AddParameter("valueType", valueType.ToString().ToLower());
            
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update a new sensor as trigger to an event.
        /// </summary>
        /// <param name="eventId">The id of the event to add the trigger to</param>
        /// <param name="sensorId">The id of the sensor to monitor</param>
        /// <param name="value">The value to trigger on</param>
        /// <param name="edge">Rising or falling edge? Accepted values are: True: Rising edge.When the new value biigger then old value Null: Equal.When new value == old value False: Falling edge.When new value less then old value</param>
        /// <param name="valueType">Value from enum ValueTyp (The type of the value. Accepted values are: temp, humidity, wgust (wind gust), wavg (wind average), watt (power), ill (illumination), rrate (rain rate) and unknown)</param>
        /// <param name="reloadValue">(optional) This value sets how much the value must drift before the trigger could be triggered again. This is useful for sensors that swings in the temperature. Default value is one degree. Example: If the trigger is set to 25 degree and reloadValue is 1, then the temperature needs to reach below 24 or above 26 for this trigger to trigger again. Must be in the interval 0.1 - 15</param>
        /// <param name="scale">(optional) The value scale (unit type) used.</param>
        /// <param name="id">The id of the trigger. Leave empty to create a new trigger</param>
        /// <returns>Class with result and errorinfo</returns>
        public ResultAdd EventSetSensorTrigger(string eventId, string sensorId, byte value,
            bool ?edge, ValueType valueType, int ?reloadValue = null, string scale = null, string id =null)
        {
            var request = new RestRequest("json/event/setSensorTrigger", Method.GET);
            if(id != null)
            {
                request.AddParameter("id", id);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("sensorId", sensorId);
            request.AddParameter("value", value.ToString());
            if (edge==true)
            {
                request.AddParameter("edge", "1");
            }else if( edge == false){
                request.AddParameter("edge", "-1");
            }else if( edge == null){
                request.AddParameter("edge", "0");
            }
            request.AddParameter("valueType", valueType.ToString().ToLower());
            if (reloadValue != null)
            {
                request.AddParameter("reloadValue", reloadValue);
            }
            if (scale != null)
            {
                request.AddParameter("scale", scale);
            }
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update a the sun as condition to an event.
        /// </summary>
        /// <param name="eventId">The id of the event to add the condition to</param>
        /// <param name="sunStatus">If the sun must be up or down true=up and false=down</param>
        /// <param name="sunriseOffset">Number of minutes before or after the sunrise</param>
        /// <param name="sunsetOffset">Number of minutes before or after the sunset</param>
        /// <param name="group">The condition group to add this condition to. All conditions in a group must be true for the action to happen. If this is not set or null a new group will be created.</param>
        /// <param name="id">The id of the condition. Leave empty to create a new condition</param>
        /// <returns>Class contianing result id and error info</returns>
        public ResultAdd EventSetSuntimeCondition(string eventId, bool sunStatus,
            byte sunriseOffset, byte sunsetOffset, string group=null, string id=null)
        {
            var request = new RestRequest("json/event/setSuntimeCondition", Method.GET);
            if( id != null)
            {
                request.AddParameter("id", id);
            }
            if (group != null)
            {
                request.AddParameter("group", group);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("sunStatus", (sunStatus) ? "1" : "0");
            request.AddParameter("sunriseOffset", sunriseOffset.ToString());
            request.AddParameter("sunsetOffset", sunsetOffset.ToString());
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }
        
        /// <summary>
        /// Adds or update a sunset/sunrise as trigger to an event
        /// </summary>
        /// <param name="eventId">The id of the event to add the trigger to</param>
        /// <param name="clientId">The id of the client to use the sunset/sunrise time from</param>
        /// <param name="sunStatus">If the sun must be up or down true=up and false=down</param>
        /// <param name="offset">Minutes before or after (use -) sunrise/sunset</param>
        /// <param name="id">The id of the trigger. Leave empty to create a new trigger</param>
        /// <returns>Class contianing result id and error info</returns>
        public ResultAdd EventSetSuntimeTrigger(string eventId, string clientId,
            bool sunStatus, byte offset, string id=null)
        {
            var request = new RestRequest("json/event/setSuntimeTrigger", Method.GET);
            if( id != null)
            {
                request.AddParameter("id", id);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("clientId", clientId);
            request.AddParameter("sunStatus", (sunStatus) ? "1" : "0");
            request.AddParameter("offset", offset.ToString());
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update a time as condition to an event
        /// </summary>
        /// <param name="eventId">The id of the event to add the trigger to</param>
        /// <param name="fromHour">A value between 0-23</param>
        /// <param name="fromMinute">A value between 0-59</param>
        /// <param name="toHour">A value between 0-23</param>
        /// <param name="toMinute">A value between 0-59</param>
        /// <param name="group">The condition group to add this condition to. All conditions in a group must be true for the action to happen. If this is not set or null a new group will be created.</param>
        /// <param name="id">The id of the condition. Leave empty to create a new condition</param>
        /// <returns>Class contianing result id and error info</returns>
        public ResultAdd EventSetTimeCondition(string eventId, byte fromHour,
            byte fromMinute, byte toHour, byte toMinute, string group=null, string id=null)
        {
            var request = new RestRequest("json/event/setTimeCondition", Method.GET);
            if( id != null)
            {
                request.AddParameter("id", id);
            }
            if( group != null)
            {
                request.AddParameter("group", group);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("fromHour", fromHour.ToString());
            request.AddParameter("fromMinute", fromMinute.ToString());
            request.AddParameter("toHour", toHour.ToString());
            request.AddParameter("toMinute", toMinute.ToString());
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update a time as trigger to an event.
        /// </summary>
        /// <param name="eventId">The id of the event to add the trigger to</param>
        /// <param name="clientId">The id of the client to use the timezone from</param>
        /// <param name="hour">A value between 0-23</param>
        /// <param name="minute">A value between 0-59</param>
        /// <param name="id">The id of the trigger. Leave empty to create a new trigger</param>
        /// <returns>Class contianing result id and error info</returns>
        public ResultAdd EventSetTimeTrigger(string eventId, string clientId, byte hour,
            byte minute, string id=null)
        {
            var request = new RestRequest("json/event/setTimeTrigger", Method.GET);
            if( id != null)
            {
                request.AddParameter("id", id);
            }
            request.AddParameter("eventId", eventId);
            request.AddParameter("clientId", clientId);
            request.AddParameter("hour", hour.ToString());
            request.AddParameter("minute", minute.ToString());
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        public ResultAdd EventSetURLAction(string eventId, string url, DelayPolicy delayPolicy,
            string id=null, byte ?delay=null)//TODO: need pro, to test this
        {
            var request = new RestRequest("json/event/setURLAction", Method.GET);
            if( id != null)
            {
                request.AddParameter("id", id);
            }
            request.AddParameter("delay", delay.ToString());

            request.AddParameter("eventId", eventId);
            request.AddParameter("url", url);
            request.AddParameter("delayPolicy", delayPolicy.ToString());
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Adds or update weekdays as conditions to an event.
        /// </summary>
        /// <param name="eventId">The id of the event to add the trigger to</param>
        /// <param name="weekdays">List off day enums for every day that should be triggerd on</param>
        /// <param name="group">If group is set you get an AND condition if not you get an ELES condition</param>
        /// <param name="id">The id of the condition. Leave empty to create a new condition</param>
        /// <returns></returns>
        public ResultAdd EventSetWeekdaysCondition(string eventId, List<Days>weekdays, 
            string group=null, string id = null)
        {
            string days = "";
            var request = new RestRequest("json/event/setWeekdaysCondition", Method.GET);
            if (id != null)
            {
                request.AddParameter("id", id);
            }
            if( group != null)
            {
                request.AddParameter("group", group);
            }
            request.AddParameter("eventId", eventId);
            foreach(int day in weekdays)
            {
                days += day.ToString() + ',';
            }            
            request.AddParameter("weekdays", days.TrimEnd(','));
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Removes an action.
        /// </summary>
        /// <param name="id">The id of the action to remove</param>
        /// <returns>Class with result data</returns>
        public Result EventRemoveAction(string id)
        {
            var request = new RestRequest("json/event/removeAction", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Removes a condition.
        /// </summary>
        /// <param name="id">The id of the condition</param>
        /// <returns>Class with result data</returns>
        public Result EventRemoveCondition(string id)
        {
            var request = new RestRequest("json/event/removeCondition", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Removes an event.
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <returns>Class with result data</returns>
        public Result EventRemoveEvent(string id)
        {
            var request = new RestRequest("json/event/removeEvent", Method.GET);
            request.AddParameter("id", id);            
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Removes a trigger.
        /// </summary>
        /// <param name="id">The id of the trigger that will be removed</param>
        /// <returns>a class with result data</returns>
        public Result EventRemoveTrigger(string id)
        {
            var request = new RestRequest("json/event/removeTrigger", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        //Group
        /// <summary>
        /// Adds a new group with devices and connects it to a client. The client must be editable for this to work. Please note that groups are devices as well.This means that all device/* commands will work for groups too.
        /// </summary>
        /// <param name="clientId">The id of the client</param>
        /// <param name="name">The name of the group</param>
        /// <param name="devices">List off Devises with all devises that should be in the group</param>
        /// <returns>a class contianing result id and error info</returns>
        public ResultAdd GroupAdd(string clientId, string name, List<Device> devices)
        {
            string devicesString = "";
            var request = new RestRequest("json/group/add", Method.GET);
            request.AddParameter("clientId", clientId);
            request.AddParameter("name", name);
            foreach(Device item in devices)
            {
                devicesString += item.id + ",";
            }
            devicesString = devicesString.TrimEnd(',');
            request.AddParameter("devices", devicesString);
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Removes a group.
        /// </summary>
        /// <param name="id">The id of the group</param>
        /// <returns>Class contianing result id and error info</returns>
        public Result GroupRemove(string id)
        {
            var request = new RestRequest("json/group/remove", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        //Scheduler
        /// <summary>
        /// Retrieves info about a specific job
        /// </summary>
        /// <param name="id">The job id</param>
        /// <returns>Class with SchdulerData</returns>
        public Scheduler SchedulerJobInfo(string id)
        {
            var request = new RestRequest("json/scheduler/jobInfo", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Scheduler> response = client.Execute<Scheduler>(request);           
            return response.Data;
        }

        /// <summary>
        /// Lists all jobs. The list is sorted by nextRunTime. If nextRunTime is 0 it means the job will not be run at all.
        /// </summary>
        /// <returns>List off jobs</returns>
        public List<Job> SchedulerJobList()
        {
            var request = new RestRequest("xml/scheduler/jobList", Method.GET);
            IRestResponse<List<Job>> response = client.Execute<List<Job>>(request);           
            return response.Data;
        }

        /// <summary>
        /// Removes a job
        /// </summary>
        /// <param name="id">The job id</param>
        /// <returns>true is suscess else error messges</returns>
        public Result SchedulerRemoveJob(string id)
        {
            var request = new RestRequest("json/scheduler/removeJob", Method.GET);
            request.AddParameter("id", id);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Creates or updates a job. Set id to 0 if you want to create a new job. If you are creating a new job the deviceId must also be set. The deviceId can only be set upon creating a new job. When updating an existing job the deviceId parameter must be omitted. Note! Only schedulable devices can be scheduled.
        /// </summary>
        /// <param name="deviceId">The device id to schedule. Only valid when creating a new job</param>
        /// <param name="method"></param>
        /// <param name="methodValue">Only required for methods that requires this.</param>
        /// <param name="type">Enum with allowed values</param>
        /// <param name="hour">A value between 0-23</param>
        /// <param name="minute">A value between 0-59</param>
        /// <param name="offset">A value between -1439-1439. This is only used when type is either 'sunrise' or 'sunset'</param>
        /// <param name="randomInterval">Number of minutes after the specified time to randomize.</param>
        /// <param name="retries">	If the client is offline, this specifies the number of times to retry executing the job before consider the job as failed.</param>
        /// <param name="retryInterval">The number if minutes between retries. Example: If retries is 3 and retryInterval is 5 the scheduler will try executing the job every five minutes for fifteen minutes.</param>
        /// <param name="reps">Number of times to resend the job to the client, for better reliability</param>
        /// <param name="weekdays">Arry with selected days to runt on</param>
        /// <param name="active">Is the job active or paused?</param>
        /// <param name="id">The job id, when updating an existing job</param>
        /// <returns>Class contianing result id and error info</returns>
        public ResultAdd SchedulerSetJob( string deviceId, string method, string methodValue, JobType type,
            byte hour, byte minute, int offset, byte randomInterval, byte retries, byte retryInterval,
            byte reps, List<Days> weekdays, bool active=true, string id=null)
        {
            string weekdayString = "";
            var request = new RestRequest("json/scheduler/setJob", Method.GET);
            if(id != null)
            {
             request.AddParameter("id", id);
            }
            foreach(int item in weekdays)
            {
                weekdayString += item.ToString() + ",";
            }
            request.AddParameter("deviceId", deviceId);
            request.AddParameter("method", method);
            request.AddParameter("methodValue", methodValue);
            request.AddParameter("type", type.ToString().ToLower());
            request.AddParameter("hour", hour);
            request.AddParameter("minute", minute);
            request.AddParameter("offset", offset);
            request.AddParameter("randomInterval", randomInterval);
            request.AddParameter("retries", retries);
            request.AddParameter("retryInterval", retryInterval);
            request.AddParameter("reps", reps);
            request.AddParameter("active", (active) ? "1" : "0");
            request.AddParameter("weekdays", weekdayString.TrimEnd(','));
            IRestResponse<ResultAdd> response = client.Execute<ResultAdd>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        //Sensors

        /// <summary>
        /// Returns a list of all sensors associated with the current user
        /// </summary>
        /// <param name="includeIgnored">Set to True to include ignored sensors</param>
        /// <param name="includeValues">Set to True to include the last value for each sensor</param>
        /// <param name="includeScale">Set to True to include the scale types for values (only valid if combined with 'includeValues'), this will return values in a separate list</param>
        /// <param name="useAlternativeData">BETA Use sensor data from alternative storage, this parameter will be REMOVED in the future (alternative storage will always be used) BETA</param>
        /// <returns>List off Sensor Data</returns>
        public List<Sensor> SensorsList(bool includeIgnored=false, bool includeValues=false,
            bool includeScale=false, bool useAlternativeData = true)
        {
            const string startToken = "<sensor ";
            const string stopToken = "</sensor>";
            int begining = 0;
            int end = 0;
            XmlDeserializer d = new XmlDeserializer();

            var request = new RestRequest("xml/sensors/list", Method.GET);            
            request.AddParameter("includeIgnored", (includeIgnored) ? "1" : "0");
            request.AddParameter("includeValues", (includeValues) ? "1" : "0");
            request.AddParameter("includeScale", (includeScale) ? "1" : "0");
            request.AddParameter("useAlternativeData", (useAlternativeData) ? "1" : "0");
            IRestResponse<List<Sensor>> response = client.Execute<List<Sensor>>(request);
            string content = response.Content;
            if (includeValues && includeScale)
            {
                for (int i = 0; i < response.Data.Count; i++)
                {
                    begining = response.Content.IndexOf(startToken, end);
                    end = response.Content.IndexOf(stopToken, begining) + (stopToken.Length);
                    response.Content = response.Content.Substring(begining, (end - begining));
                    response.Data[i].data = d.Deserialize<List<Data>>(response);
                    response.Content = content;
                }
            }            
            return response.Data;
        }

        //Sensor

        /// <summary>
        /// Returns information about a specific sensor. You are not allowed to call this function more often than every 10 minutes.
        /// </summary>
        /// <param name="id">The id of the sensor</param>
        /// <param name="useAlternativeData">BETA Use sensor data from alternative storage, this parameter will be REMOVED in the future (alternative storage will always be used) BETA</param>
        public Sensor SensorInfo(string id, bool useAlternativeData = true)
        {
            var request = new RestRequest("json/sensor/info", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("useAlternativeData", (useAlternativeData)?"1":"0");
            IRestResponse<Sensor> response = client.Execute<Sensor>(request);            
            return response.Data;
        }

        /// <summary>
        /// Mark a sensor as 'ignored'. Ignored sensors will not show up in sensors/list if not explicit set to do so.
        /// </summary>
        /// <param name="id">The id of the sensor</param>
        /// <param name="ignore">TRue to ignore the sensor, False to inlude it again</param>
        /// <returns>True on success</returns>
        public Result SensorSetIgnore(string id, bool ignore)
        {
            var request = new RestRequest("json/sensor/setIgnore", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("ignore", (ignore) ? "1" : "0");
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Renames a sensor
        /// </summary>
        /// <param name="id">The id of the sensor</param>
        /// <param name="name">The new name</param>
        /// <returns>True on success</returns>
        public Result SensorSetName(string id, string name)
        {
            var request = new RestRequest("json/sensor/setName", Method.GET);
            request.AddParameter("id", id);
            request.AddParameter("name", name);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        //User
        /// <summary>
        /// Change the user's password. This function must be called over https
        /// </summary>
        /// <param name="currentPassword">The current password</param>
        /// <param name="newPassword">The new password</param>
        /// <returns></returns>
        public Result UserChangePassword(string currentPassword, string newPassword)
        {
            var request = new RestRequest("json/user/changePassword", Method.GET);
            request.AddParameter("currentPassword", currentPassword);
            request.AddParameter("newPassword", newPassword);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Remove connection to this phone/user
        /// </summary>
        /// <param name="token">The unique phone id token used for pushing</param>
        /// <returns>True if sucess else error messages</returns>
        public Result UserDeletePushToken(string token)
        {
            var request = new RestRequest("json/user/deletePushToken", Method.GET);
            request.AddParameter("token", token);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// List all connected phones for this user
        /// </summary>
        /// <returns>a class with phone information</returns>
        public List<Phone> UserListPhones()
        {
            var request = new RestRequest("xml/user/listPhones", Method.GET);
            IRestResponse<List<Phone>> response = client.Execute<List<Phone>>(request);
            return response.Data;
        }

        /// <summary>
        /// Gets information about the currently logged in user
        /// </summary>
        /// <returns>A class with User Info</returns>
        public User UserProfile()
        {
            var request = new RestRequest("json/user/profile", Method.GET);
            IRestResponse<User> response = client.Execute<User>(request);
            return response.Data;
        }

        /// <summary>
        /// Adds a phone in the system to be pushable
        /// </summary>
        /// <param name="token">The unique phone id token used for pushing</param>
        /// <param name="name">The name of this phone</param>
        /// <param name="model">The model of this phone</param>
        /// <param name="manufacturer">The manufacturer of this phone</param>
        /// <param name="osVersion">The os-version of this phone</param>
        /// <param name="deviceId">The device-id of this phone</param>
        /// <param name="pushServiceId">The ID push_service_id</param>
        /// <returns>True on success</returns>
        public Result UserRegisterPushToken(string token, string name, string model, 
            string manufacturer, string osVersion, string deviceId, string pushServiceId)// Should only be done from a phone will not be implimented
        {
            var request = new RestRequest("json/user/registerPushToken", Method.GET);
            request.AddParameter("token", token);
            request.AddParameter("name", name);
            request.AddParameter("model", model);
            request.AddParameter("manufacturer", manufacturer);
            request.AddParameter("osVersion", osVersion);
            request.AddParameter("deviceId", deviceId);
            request.AddParameter("pushServiceId", pushServiceId);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Send a test push notification
        /// </summary>
        /// <param name="phoneId">The id of the phone to send the push notification to</param>
        /// <param name="message">The message to send</param>
        /// <returns>True if message sent without Error</returns>
        public Result UserSendPushTest(string phoneId, string message)
        {
            var request = new RestRequest("json/user/sendPushTest", Method.GET);
            request.AddParameter("phoneId", phoneId);
            request.AddParameter("message", message);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

        /// <summary>
        /// Don't push to this phone/user any more
        /// </summary>
        /// <param name="token">The unique phone id token used for pushing</param>
        /// <returns>True on success</returns>
        public Result UserUnregisterPushToken(string token)
        {
            var request = new RestRequest("json/user/unregisterPushToken", Method.GET);
            request.AddParameter("token", token);
            IRestResponse<Result> response = client.Execute<Result>(request);
            if (response.Data.error == null)
            {
                response.Data.success = true;
            }
            return response.Data;
        }

    }

    //----------------------------- suported classes ------------------------------

    public class Client
    {
        public string id { get; set; }
        public string ip { get; set; }
        public string uuid { get; set; }
        public string name { get; set; }
        public bool online { get; set; }
        public bool editable { get; set; }
        public int extensions { get; set; }
        public int version { get; set; }
        public string type { get; set; }
        //----------------- extra -------------------------
        public string longitude { get; set; }
        public string latitude { get; set; }
        public bool latestversion { get; set; }
        public string downloadUrl { get; set; }
        public string sunrise { get; set; }
        public string sunset { get; set; }
        public string timezone { get; set; }
        public bool timezoneAutodetected { get; set; }
        public int tzoffset { get; set; }
        public string transports { get; set; }
        public string features { get; set; }
        public string error { get; set; }

        public void Clean()
        {
            char[] trim = { '\n', '\t' };
            id = (id != null) ? id.Trim(trim) : null;
            ip = (ip != null) ? ip.Trim(trim) : null;
            uuid = (uuid != null) ? uuid.Trim(trim) : null;
            name = (name != null) ? name.Trim(trim) : null;
            type = (type != null) ? type.Trim(trim) : null;
            longitude = (longitude != null) ? longitude.Trim(trim) : null;
            latitude = (latitude != null) ? latitude.Trim(trim) : null;
            downloadUrl = (downloadUrl != null) ? downloadUrl.Trim(trim) : null;
            sunrise = (sunrise != null) ? sunrise.Trim(trim) : null;
            sunset = (sunset != null) ? sunset.Trim(trim) : null;
            timezone = (timezone != null) ? timezone.Trim(trim) : null;
            transports = (transports != null) ? transports.Trim(trim) : null;
            features = (features != null) ? features.Trim(trim) : null;
        }
    }

    public class Device
    {
        public string id { get; set; }
        public string clientDeviceId { get; set; }
        public string name { get; set; }
        public int state { get; set; }
        public string statevalue { get; set; }
        public int methods { get; set; }
        public string type { get; set; }
        public string client { get; set; }
        public string clientName { get; set; }
        public string protocol { get; set; }
        public string model { get; set; }
        public bool online { get; set; }
        public bool editable { get; set; }
        public bool ignored { get; set; }
        //----------------- extra -------------------------
        public string longitude { get; set; }
        public string latitude { get; set; }
        public string timezone { get; set; }
        public int tzoffset { get; set; }
        public string transport { get; set; }
  
        public string error { get; set; }
        public class parameter
        {
            public string house { get; set; }
            public string unit { get; set; }
            //<parameter value = "37364722" name="house"/>       
            //<parameter value = "16" name="unit"/>
        }
            public void Clean()
        {
            char[] trim = { '\n', '\t' };
            id = (id != null) ? id.Trim(trim) : null;
            name = (name != null) ? name.Trim(trim) : null;
            statevalue = (statevalue != null) ? statevalue.Trim(trim) : null;
            type = (type != null) ? type.Trim(trim) : null;
            client = (client != null) ? client.Trim(trim) : null;
            protocol = (protocol != null) ? protocol.Trim(trim) : null;
            model = (model != null) ? model.Trim(trim) : null;
            error = (error != null) ? error.Trim(trim) : null;

            longitude = (longitude != null) ? longitude.Trim(trim) : null;
            latitude = (latitude != null) ? latitude.Trim(trim) : null;
            timezone = (timezone != null) ? timezone.Trim(trim) : null;
            transport = (transport != null) ? transport.Trim(trim) : null;
        }
    }   

    public class Sensor
    {
        //{"sensor":[{"id":"9939514","name":"Bratt\u00e5s","lastUpdated":1484033964,"ignored":0,"client":"228076","clientName":"Bratt\u00e5s","online":"1","editable":1,"battery":254,"keepHistory":0,"protocol":"fineoffset","model":"temperaturehumidity","sensorId":"28","data":[{"name":"temp","value":"20.3","scale":"0","lastUpdated":1484033964.4941,"max":"23.9","maxTime":1483812257.651,"min":"14.8","minTime":1483273038.4611},{"name":"humidity","value":"41","scale":"0","lastUpdated":1484033964.4941,"max":"51","maxTime":1483281150.3057,"min":"37","minTime":1483654725.3425}]},{"id":"9939523","name":"Camilla","lastUpdated":1484033968,"ignored":0,"client":"228076","clientName":"Bratt\u00e5s","online":"1","editable":1,"battery":254,"keepHistory":0,"protocol":"fineoffset","model":"temperaturehumidity","sensorId":"29","data":[{"name":"temp","value":"18.8","scale":"0","lastUpdated":1484033968.685,"max":"23.5","maxTime":1483565880.8896,"min":"15.7","minTime":1483559449.2741},{"name":"humidity","value":"39","scale":"0","lastUpdated":1484033968.685,"max":"50","maxTime":1483281341.231,"min":"33","minTime":1483646135.4722}]}]}
        public string id { set; get; }
        public string name { set; get; }
        public string lastUpdated { set; get; }
        //public bool ignored { set; get; }
        public string ignored { set; get; }
        public string client { set; get; }
        public string clientName { set; get; }
        //public bool online { set; get; }
        public string online { set; get; }
        //public bool editable { set; get; }
        public string editable { set; get; }
        //public int battery { set; get; }
        public string battery { set; get; }
        //public bool keepHistory { set; get; }c
        public string keepHistory { set; get; }
        public string protocol { set; get; }
        public string model { set; get; }
        //public int sensorId { set; get; }
        public string sensorId { set; get; }
        public List<Data> data { set; get; }
        public string error { set; get; }


    }

    public class Data
    {
        //  lastUpdated="1484035212.9514" name="temp" minTime="1483273038.4611" min="14.8" maxTime="1483812257.651" max="23.9" scale="0" value="20.4"/>
        public string lastUpdated { set; get; }
        public string name { set; get; }
        public string minTime { set; get; }
        //public int min { set; get; }
        public string min { set; get; }
        public string maxTime { set; get; }
        //public int max { set; get; }
        public string max { set; get; }
        //public int scale { set; get; }
        public string scale { set; get; }
        //public int value { set; get; }
        public string value { set; get; }
    }
    public class Event
    {        
        public bool active { get; set; }
        public int minRepeatInterval { get; set; }
        public string description { get; set; }
        public string id { get; set; }       
        public List<Trigger> Trigger { get; set; }  // if list then it q´works with info and if class then it works with EventList
        public List<Condition> Condition { get; set; } // if list then it q´works with info and if class then it works with EventList
        public List<Action> Action { get; set; } // if list then it q´works with info and if class then it works with EventList
        public string error { get; set; }
        
    }

    public class Condition
    {
        public int method { get; set; }
        public string deviceId { get; set; }
        public string id { get; set; }
        public string type { get; set; }
        public string group { get; set; }
        public string error { get; set; }

    }

    public class Trigger
    {
        public string id { get; set; }
        public string clientId { get; set; }
        public string type { get; set; }
        public string deviceId { get; set; }
        public int method { get; set; }
        public string error { get; set; }
    }

    public class Action
    {
        public string id { get; set; }
        public string type { get; set; }
        public string deviceId { get; set; }
        public int method { get; set; }
        public DelayPolicy delayPolicy { get; set; }
        public int delay { get; set; }
        public int repeats { get; set; }
        public int value { get; set; }
        public string error { get; set; }
    }

    public class User
    {
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public bool pro { get; set; }
        public string locale { get; set; }
        public bool admin { get; set; }
        public string error { get; set; }
    }

    public class Phone
    {
        public string displayName { set; get; }
        public bool active { set; get; }
        public int pushServiceId { set; get; }
        public string deviceId { set; get; }
        public string osVersion { set; get; }
        public string manufacturer { set; get; }
        public string model { set; get; }
        public string name { set; get; }
        public string token { set; get; }
        public string id { set; get; }
        public string error { get; set; }
    }

    public class History
    {
        //{"history":[{"ts":1483137844,"state":16,"stateValue":0,"origin":"TelLIVE Remote","successStatus":0}
        public string ts { set; get; }
        public string state { set; get; }
        public int stateValue { set; get; }
        public string origin { set; get; }
        public bool successStatus { set; get; }        
        public string error { set; get; }
    }

    public class ExtraHistory
    {
        public List<History> history {set; get;}
        public string timezone { set; get; }
        public bool timezoneAutodetected { set; get; }
        public int tzoffset { set; get; }
        public string error { set; get; }
    }
    public class Scheduler
    {
        //{"id":"1562501","deviceId":"1479553","method":"1","methodValue":"0","nextRunTime":1483693140,"type":"time","hour":"9","minute":"59","offset":0,"randomInterval":0,"retries":3,"retryInterval":5,"reps":1,"active":1,"weekdays":"5,7"}
        public string id { get; set; }
        public string deviceId { get; set; }
        public string method { get; set; }
        public string methodValue { get; set; }
        public string nextRunTime { get; set; }
        public JobType type { get; set; }
        public int hour { get; set; }
        public int minute { get; set; }
        public int offset { get; set; }
        public int randomInterval { get; set; }
        public int retries { get; set; }
        public int retryInterval { get; set; }
        public int reps { get; set; }
        public bool active { get; set; }
        public string weekdays { get; set; }
        public string error { get; set; }
    }

    public class Job
    {
        //<job id="1562501" deviceId="1479553" method="1" methodValue="0" nextRunTime="1483693140" type="time" hour="9" minute="59" offset="0" randomInterval="0" retries="3" retryInterval="5" reps="1" active="1" weekdays="5,7"
        public string id { get; set; }
        public string deviceId { get; set; }
        public string method { get; set; }
        public string methodValue{ get; set; }
        public string nextRunTime { get; set; }
        public string type { get; set; }
        public string hour { get; set; }
        public string minute { get; set; }
        public string offset { get; set; }
        public string randomInterval { get; set; }
        public string retries { get; set; }
        public string retryInterval { get; set; }
        public string reps { get; set; }
        public string active { get; set; }
        public string weekdays { get; set; }
    }
    public class Result
    {

        public bool success { get; set; }
        public string error { get; set; }
    }

    public class ResultAdd
    {
        public string group { get; set; }
        public string id { get; set; }
        public bool success { get; set; }
        public string error { get; set; }
    }
}