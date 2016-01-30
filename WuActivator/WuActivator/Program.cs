using System;
using EloBuddy;
using EloBuddy.SDK.Events;

namespace WuActivator
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            try
            {
                new Activator();
                Chat.Print("WuActivator loaded, by WujuSan");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
