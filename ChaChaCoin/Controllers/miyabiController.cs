using System;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Miyabi;
using Miyabi.Asset.Client;
using Miyabi.Asset.Models;
using Miyabi.Cryptography;
using Miyabi.Common.Models;
using Miyabi.ClientSdk;
using WalletService;
using Miyabi.Serialization.Json;
using System.Text.Json;
using System.Diagnostics;
using System.ComponentModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChaChaCoin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class miyabiController : ControllerBase
    {

        //recieve send_info
        [DataContract]
        public class Transactioninfo
        {  
            [DataMember]
            public string my_privatekey { get; set; }
            [DataMember]
            public string opponet_pubkey { get; set; }
            [DataMember]
            public string send_amount { get; set;}
        }
        //send resulet responce
        [DataContract]
        public class Result
        {
            [DataMember]
            public  int code { get; set; }
            [DataMember]
            public string transactionId { get; set; }
            [DataMember]
            public string result { get; set; }
        }
        //coin amount
        [DataContract]
        public class Showamount
        {
            [DataMember]
            public string my_publickey { get; set; }
        }
        [DataContract]
        public class Showresult
        {
            [DataMember]
            public  string coin_amount { get; set; }
        }
         //privatekey regist
        [DataContract]
        public class RegistName
        {
            [DataMember]
            public string nickname { get; set; }
        }

        [DataContract]
        public class  getkeypair
        {
            [DataMember]
            public string PrivateKey { get; set; }
            [DataMember]
            public string PublicKey { get; set; }

        }
        //裏オプション（プライベートキーの手動登録）
        [DataContract]
        public class BeforeParse
        {
            [DataMember]
            public string BeforeParsePrivateKey { get; set; }

        }
        [DataContract]
        public class AfterParse
        {
            [DataMember]
            public string AfterParsepublickey { get; set; }

        }
        // GET: api/<miyabiController>/
        /*[HttpGet]
        [Route("aaa")]
        public async Task<string> Get()
        {*/

        //return new string[] { "value1", "value2" };
        //}

        // GET api/<miyabiController>/5
        /*[HttpGet]
        [Route("aaa")]
        public string Get()
        {
            */
            /* Address opponetpublickey = Inputjudgement2("0338f9e2e7ad512fe039adaa509f7b555fb88719144945bd94f58887d8d54fed78");
           Debug.WriteLine(opponetpublickey);
           string ParsePublicKey = Convert.ToString(opponetpublickey);
           Debug.WriteLine(ParsePublicKey);
           return ParsePublicKey;
         //test code
           decimal coinamount = 0m;
             WAction walletservice = new WAction();
             coinamount = await walletservice.ShowAsset(showdata.my_publickey);

             Showresult amountresult = new Showresult();
             amountresult.coin_amount = coinamount;
             string amountjson = JsonSerializer.Serialize(amountresult);
             return amountjson;*/

        //}

        // POST api/<miyabiController>
        /// <summary>
        /// coinsend application
        /// </summary>
        /// <param name="info"></param>
        /// <returns>return resultjson</returns>
        /// <returns>return successresultjson</returns>

        //post /api/<miyabiController>/send
        [Route("send")]
        [HttpPost]
        public async Task<string>  Coinsend([FromBody] Transactioninfo info)
        {
            AssetTypesRegisterer.RegisterTypes();
            WAction walletservice = new WAction();
            KeyPair privatekey = walletservice.GetKeyPair(info.my_privatekey);
            if(privatekey == null)
            {
                // comment:プライベートキーが適正ではありません！
                Result txresult = new Result();
                txresult.code = 1;
                txresult.transactionId = string.Empty;
                txresult.result = string.Empty;
                string resultjson = JsonSerializer.Serialize(txresult);
                return resultjson;
            }
            Address mypublickey = Inputjudgement1(privatekey);
            if(mypublickey == null)
            {
                //comment:パブリックキーを適正に変換できませんでした！
                Result txresult = new Result();
                txresult.code = 2;
                txresult.transactionId = string.Empty;
                txresult.result = string.Empty;
                string resultjson = JsonSerializer.Serialize(txresult);
                return resultjson;
            }
            Address opponetpublickey = Inputjudgement2(info.opponet_pubkey);
            if(opponetpublickey == null)
            {
                // comment:入力された送信相手のパブリックキーが不適正です！入力値を再確認してください!
                Result txresult = new Result();
                txresult.code = 3;
                txresult.transactionId =  string.Empty;
                txresult.result = string.Empty;
                string resultjson = JsonSerializer.Serialize(txresult);
                return resultjson;
            }
            decimal amount = Inputnumjudgement(info.send_amount);
            if (amount == 0m)
            {
                //comment:数字ではない文字が入力されています！数字に直してください
                Result txresult = new Result();
                txresult.code = 4;
                txresult.transactionId = string.Empty;
                txresult.result = string.Empty;
                string resultjson = JsonSerializer.Serialize(txresult);
                return resultjson;
            }
            (string transaction, string result) = await walletservice.Send(privatekey, mypublickey, opponetpublickey, amount);
            Result successtxresult = new Result();
            successtxresult.code = 5;
            successtxresult.transactionId = transaction;
            successtxresult.result = result;
            string successresultjson = JsonSerializer.Serialize(successtxresult);
            return successresultjson;
        }
        /// <summary>
        /// showcoin
        /// </summary>
        /// <param name="showdata"></param>
        /// <returns>return amountjson</returns>
        //post /api/<miyabiController>/show
        [Route("show")]
        [HttpPost]
        public async Task<string> showcoin([FromBody] Showamount showdata)
        {
            decimal coinamount = 0m;
            WAction walletservice = new WAction();
            coinamount = await walletservice.ShowAsset(showdata.my_publickey);
            Showresult amountresult = new Showresult();
            amountresult.coin_amount = Convert.ToString(coinamount);    
            string amountjson = JsonSerializer.Serialize(amountresult);
            return amountjson;
        }
        /// <summary>
        /// Get KeyPair at first 
        /// </summary>
        /// <param name="showdata"></param>
        /// <returns></returns>
        //post /api/<miyabiController>/regist
        [Route("regist")]
        [HttpPost]
        public string Get_Keypair([FromBody]RegistName registname)
        {
            AssetTypesRegisterer.RegisterTypes();
            Process process = new Process();
            string arg = registname.nickname ;//argumenrt

            ProcessStartInfo psinfo = new ProcessStartInfo()
            {
                FileName = "/usr/local/bin/Generate_Privekey.sh",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                Arguments = "\t" + arg,
            };
            Process p = Process.Start(psinfo);

            p.WaitForExit();
            string privatekey = p.StandardOutput.ReadToEnd();
            privatekey = privatekey.Substring(0, privatekey.Length - 1);
            p.Close();

            WAction walletservice = new WAction();
            KeyPair privatekey1 = walletservice.GetKeyPair(privatekey);
            if (privatekey1 == null)
            {
                // comment:プライベートキーが正しく取得できませんでした！
                getkeypair getkeypair = new getkeypair();
                getkeypair.PrivateKey = string.Empty;
                getkeypair.PublicKey = string.Empty;
                string resultjson = JsonSerializer.Serialize(getkeypair);
                return resultjson;
            }
            Address mypublickey = Inputjudgement1(privatekey1);
            if (mypublickey == null)
            {
                //comment:パブリックキーに変換できませんでした！
                getkeypair getkeypair = new getkeypair();
                getkeypair.PrivateKey = string.Empty;
                getkeypair.PublicKey = string.Empty;
                string resultjson = JsonSerializer.Serialize(getkeypair);
                return resultjson;
            }
            string ParsePublicKey = Convert.ToString(mypublickey);

            getkeypair keypair = new getkeypair();
            keypair.PrivateKey = privatekey;
            keypair.PublicKey = ParsePublicKey;
            string keypairjson = JsonSerializer.Serialize(keypair);
            return keypairjson;
        }
        //
        [Route("parse")]
        [HttpPost]
        public string Parse([FromBody] BeforeParse parse)
        {
            AssetTypesRegisterer.RegisterTypes();
            WAction walletservice = new WAction();
            KeyPair privatekey = walletservice.GetKeyPair(parse.BeforeParsePrivateKey);
            if (privatekey == null)
            {
                // comment:プライベートキーが適正ではありません！
                AfterParse missparse = new AfterParse();
                missparse.AfterParsepublickey = string.Empty;
                string resultjson = JsonSerializer.Serialize(missparse);
                return resultjson;
            }
            Address mypublickey = Inputjudgement1(privatekey);
            if (mypublickey == null)
            {
                //comment:パブリックキーに変換できませんでした！
                AfterParse  missparse = new AfterParse();
                missparse.AfterParsepublickey = string.Empty;
                string resultjson = JsonSerializer.Serialize(missparse);
                return resultjson;
            }
            string ParsePublicKey = Convert.ToString(mypublickey);

            AfterParse afterparse = new AfterParse();
            afterparse.AfterParsepublickey = ParsePublicKey;
            string successjson = JsonSerializer.Serialize(afterparse);
            return successjson;
        }
        /*// PUT api/<miyabiController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<miyabiController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }*/
        /// <summary>
        /// privatekey parse publickeyaddress
        /// </summary>
        /// <param name="myprivatekey"></param>
        /// <returns></returns>
        private Address Inputjudgement1( KeyPair myprivatekey)
        {
            Address parsepublickey;
            try
            {
                parsepublickey = new PublicKeyAddress(myprivatekey);
            }
            catch (Exception)
            {
                return null; 
            }
            return parsepublickey;
        }
        /// <summary>
        /// publickey string parse publickey
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private Address Inputjudgement2(string text)
        {
            Address inputaddress = null;
            try
            {
                var value = PublicKey.Parse(text);
                inputaddress = new PublicKeyAddress(value);
            }
            catch (Exception)
            {
                return null;
            }

            return inputaddress;
        }
        /// <summary>
        /// amount string parse amount
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private decimal Inputnumjudgement(string text)
        {
            decimal inputamount = 0m;

            try
            {
                inputamount = Convert.ToDecimal(text);
            }
            catch (Exception)
            {
                return 0m;
            }

            return inputamount;
        }
    }
}
