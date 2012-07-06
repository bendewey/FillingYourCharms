using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Streams;

namespace BingSimpleSearch
{
    class DeferralSample
    {



        // BAD: This code will not work because of the await
        private async void OnDataRequested(DataTransferManager sender,
                                                DataRequestedEventArgs args)
        {
            // code before await
            var file = await DownloadImage();
            // code after await
            args.Request.Data.SetBitmap(file);
            // event handler returned to caller
        }

        // BAD: This code will not work because event handler returns prematurely
        private void OnDataRequestedWithoutAsync(DataTransferManager sender,
                                                DataRequestedEventArgs args)
        {
            // (1)  code before await
            DownloadImage().ContinueWith(t =>
            {
                // (3) code after await
                args.Request.Data.SetBitmap(t.Result);
            });
            // (2) event handler returned to caller
        }

        private async void OnDataRequestedWithDeferral(DataTransferManager sender,
                                                DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            try
            {
                // code before await
                var file = await DownloadImage();
                // code after await
                args.Request.Data.SetBitmap(file);
            }
            finally
            {
                deferral.Complete();
            }
            // event handler returned to caller
        }


        private Task<RandomAccessStreamReference> DownloadImage() { return null; }













    }
}
