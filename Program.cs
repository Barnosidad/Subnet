namespace subnet
{
    class Program
    {
        public static void Main(string [] args)
        {
            Addresses a = new Addresses(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]), Convert.ToInt32(args[3]), Convert.ToInt32(args[4]));
            Console.WriteLine("DIR: " + a.ToString());
            Console.WriteLine("MSK: " + Addresses.pointF(a.Mascara));
            Console.WriteLine("IDR: " + Addresses.pointF(a.IDRed));
            Console.WriteLine("IDB: " + Addresses.pointF(a.IDBroadcast));
            Console.WriteLine("Subneteo a /" + args[5]);
            foreach(Addresses s in Addresses.subnetWith(a,Convert.ToInt32(args[5])))
            {
                Console.WriteLine("IDR: " + s.ToString() + ", IDB: " + Addresses.pointF(s.IDBroadcast));
            }
        }
    }
}