namespace WalletService
{
    using System;
    using System.Threading.Tasks;
    using System.Net.Http;
    using Miyabi;
    using Miyabi.Asset.Client;
    using Miyabi.Asset.Models;
    using Miyabi.ClientSdk;
    using Miyabi.Common.Models;
    using Miyabi.Cryptography;
    using System.Diagnostics;


    /// <summary>
    /// Wallt action class.
    /// </summary>
    public class WAction
    {
        private const string TableName = "ChaChaCoin";
        private static string ApiUrl = "https://playground.blockchain.bitflyer.com/";
 

        /// <summary>
        /// Send Asset Method
        /// </summary>
        /// <param name="client"></param>
        /// <returns>tx.Id</returns>
        public async Task<Tuple<string, string>> Send(KeyPair formerprivatekey, Address myaddress, Address opponetaddress, decimal amount)
        {
            var client = this.SetClient();
            var generalApi = new GeneralApi(client);
            var from = myaddress;
            var to = opponetaddress;
           
            // enter the send amount
            var moveCoin = new AssetMove(TableName, amount, from, to);
            var tx = TransactionCreator.SimpleSignedTransaction(
                new ITransactionEntry[] { moveCoin },
                new[] { formerprivatekey.PrivateKey });
            await this.SendTransaction(tx);
            var result = await WaitTx(generalApi, tx.Id);
            return new Tuple<string, string>(tx.Id.ToString(), result);
        }

        /// <summary>
        /// show Asset of designated publickeyAddress
        /// </summary>
        /// <param name="client"></param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<decimal> ShowAsset(string publickey)
        {
            PublicKeyAddress myaddress = null;
            var client = this.SetClient();
            var assetClient = new AssetClient(client);
            AssetTypesRegisterer.RegisterTypes();

            try
            {
                var value = PublicKey.Parse(publickey);
                myaddress = new PublicKeyAddress(value);
            }
            catch (Exception)
            {
                return 0m;
            }
            
            var result = await assetClient.GetAssetAsync(TableName, myaddress);
            return result.Value;
        }

        /// <summary>
        /// Send Transaction to miyabi blockchain
        /// </summary>
        /// <param name="tx"></param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SendTransaction(Transaction tx)
         {
            var client = this.SetClient();
            var generalClient = new GeneralApi(client);

            try
            {
                var send = await generalClient.SendTransactionAsync(tx);
                var result_code = send.Value;

                if (result_code != TransactionResultCode.Success
                   && result_code != TransactionResultCode.Pending)
                {
                    // Console.WriteLine("取引が拒否されました!:\t{0}", result_code);
                }
            }
            catch (Exception)
            {
                // Console.Write("例外を検知しました！{e}");
                return;
            }
         }
        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        public Address GetAddress(KeyPair formerprivetekey)
        {
            return new PublicKeyAddress(formerprivetekey);
        }

         /// <summary>
         /// client set method.
         /// </summary>
         /// <returns>client</returns>
        public Client SetClient()
        {
            Client client;
            var config = new SdkConfig(ApiUrl);
            client = new Client(config);
            return client;
        }
        public static async Task<string> WaitTx(GeneralApi api, ByteString id)
        {
            while (true)
            {
                var result = await api.GetTransactionResultAsync(id);
                if (result.Value.ResultCode != TransactionResultCode.Pending)
                {
                    return result.Value.ResultCode.ToString();
                }

            }
        }
        /// <summary>
        ///  プライベートキー文字列のParse
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public KeyPair GetKeyPair(string privateKey)
        {
            PrivateKey adminPrivateKey;
            try
            {
                 adminPrivateKey = PrivateKey.Parse(privateKey);
            }
            catch(Exception)
            {
                //失敗のHTTPリクエストを投げるようにする
                return null;
            }
            return new KeyPair(adminPrivateKey);
        }
    }
}