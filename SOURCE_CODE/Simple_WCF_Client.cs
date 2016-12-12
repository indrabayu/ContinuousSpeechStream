using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace ConsumeWCF
{
    [ServiceContract]
    public interface IAndroidTestService
    {
        [OperationContract]
        List<List<string>> GetText(byte[] waves);

        [OperationContract]
        string GoogleSpeechV2Using16KHz16BitMonoWave(byte[] waves);

        [OperationContract]
        List<string> GoogleNLP_Analyze_and_Annotate(string statement);

        [OperationContract]
        string GetTime();
    }

    //[Serializable]
    //[DataContract]
    //public class AndroidTestReturnType
    //{
    //    string _Item1;
    //    byte[] _Item2;

    //    [DataMember]
    //    public string Item1
    //    {
    //        get { return _Item1; }
    //        set { _Item1 = value; }
    //    }
    //    [DataMember]
    //    public byte[] Item2
    //    {
    //        get { return _Item2; }
    //        set { _Item2 = value; }
    //    }

    //    public AndroidTestReturnType() { }

    //    public AndroidTestReturnType(string Item1, byte[] Item2)
    //    {
    //        this.Item1 = Item1;
    //        this.Item2 = Item2;
    //    }
    //}

    public class Simple_WCF_Client
    {
        public const string IP = "192.168.1.2";
        public const string Binding = "HTTP";
        public const int Port = 1234;

        public static List<List<string>> GetText(byte[] waves)
        {
            return GetText(IP, Binding, Port, waves);
        }

        public static string GetTime()
        {
            return GetTime(IP, Binding, Port);
        }

        public static string GoogleSpeechV2Using16KHz16BitMonoWave(byte[] waves)
        {
            return GoogleSpeechV2Using16KHz16BitMonoWave(IP, Binding, Port, waves);
        }

        public static List<string> GoogleNLP_Analyze_and_Annotate(string statement)
        {
            return GoogleNLP_Analyze_and_Annotate(IP, Binding, Port, statement);
        }

        public static List<List<string>> GetText(string strServer, string strBinding,
                                     int nPort, byte[] waves)
        {
            ChannelFactory<IAndroidTestService> channelFactory = null;
            EndpointAddress ep = null;

            string strEPAdr = string.Empty;
            List<List<string>> result = null;
            try
            {
                switch (strBinding)
                {
                    case "HTTP":
                        BasicHttpBinding httpb = new BasicHttpBinding();
                        httpb.MaxBufferPoolSize = long.MaxValue;
                        httpb.MaxBufferSize = int.MaxValue;
                        httpb.MaxReceivedMessageSize = int.MaxValue;
                        channelFactory = new ChannelFactory<IAndroidTestService>(httpb);

                        // End Point Address
                        strEPAdr = "http://" + strServer + ":" + nPort.ToString() + "/AndroidTestService";
                        break;
                }

                // Create End Point
                ep = new EndpointAddress(strEPAdr);

                // Create Channel
                IAndroidTestService mathSvcObj = channelFactory.CreateChannel(ep);

                // Call Methods
                result = mathSvcObj.GetText(waves);

                channelFactory.Close();
            }
            catch (Exception err)
            {
                result = null; //new string[] { err.ToString() };
            }

            return result;
        }

        public static string GetTime(string strServer, string strBinding,
                                     int nPort)
        {
            ChannelFactory<IAndroidTestService> channelFactory = null;
            EndpointAddress ep = null;

            string strEPAdr = string.Empty;
            string result = null;
            try
            {
                switch (strBinding)
                {
                    case "HTTP":
                        BasicHttpBinding httpb = new BasicHttpBinding();
                        channelFactory = new ChannelFactory<IAndroidTestService>(httpb);

                        // End Point Address
                        strEPAdr = "http://" + strServer + ":" + nPort.ToString() + "/AndroidTestService";
                        break;
                }

                // Create End Point
                ep = new EndpointAddress(strEPAdr);

                // Create Channel
                IAndroidTestService mathSvcObj = channelFactory.CreateChannel(ep);

                // Call Methods
                result = mathSvcObj.GetTime();

                channelFactory.Close();
            }
            catch (Exception err)
            {
                result = err.ToString();
            }

            return result;
        }

        public static string GoogleSpeechV2Using16KHz16BitMonoWave(string strServer, string strBinding,
                                     int nPort, byte[] waves)
        {
            ChannelFactory<IAndroidTestService> channelFactory = null;
            EndpointAddress ep = null;

            string strEPAdr = string.Empty;
            string result = null;
            try
            {
                switch (strBinding)
                {
                    case "HTTP":
                        BasicHttpBinding httpb = new BasicHttpBinding();
                        channelFactory = new ChannelFactory<IAndroidTestService>(httpb);

                        // End Point Address
                        strEPAdr = "http://" + strServer + ":" + nPort.ToString() + "/AndroidTestService";
                        break;
                }

                // Create End Point
                ep = new EndpointAddress(strEPAdr);

                // Create Channel
                IAndroidTestService mathSvcObj = channelFactory.CreateChannel(ep);

                // Call Methods
                result = mathSvcObj.GoogleSpeechV2Using16KHz16BitMonoWave(waves);

                channelFactory.Close();
            }
            catch (Exception err)
            {
                result = err.ToString();
            }

            return result;
        }

        public static List<string> GoogleNLP_Analyze_and_Annotate(string strServer, string strBinding,
                                     int nPort, string statement)
        {
            ChannelFactory<IAndroidTestService> channelFactory = null;
            EndpointAddress ep = null;

            string strEPAdr = string.Empty;
            List<string> result = null;
            try
            {
                switch (strBinding)
                {
                    case "HTTP":
                        BasicHttpBinding httpb = new BasicHttpBinding();
                        httpb.OpenTimeout = TimeSpan.FromHours(1);
                        httpb.CloseTimeout = TimeSpan.FromHours(1);

                        channelFactory = new ChannelFactory<IAndroidTestService>(httpb);

                        // End Point Address
                        strEPAdr = "http://" + strServer + ":" + nPort.ToString() + "/AndroidTestService";
                        break;
                }

                // Create End Point
                ep = new EndpointAddress(strEPAdr);

                // Create Channel
                IAndroidTestService mathSvcObj = channelFactory.CreateChannel(ep);

                // Call Methods
                result = mathSvcObj.GoogleNLP_Analyze_and_Annotate(statement);

                channelFactory.Close();
            }
            catch (Exception err)
            {
                result = new List<string>(new string[] { err.ToString() });
            }

            return result;
        }
    }
}
