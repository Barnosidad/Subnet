namespace subnet
{
    class Program
    {
        public static void Main(string [] args)
        {
            Console.WriteLine("El numero de argumentos es: " + args.Length);
            Addresses a = new Addresses(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]), Convert.ToInt32(args[3]), Convert.ToInt32(args[4]));
            if(args.Length < 6) Console.WriteLine("\t\t .: Only for information purpose :.");
            Console.WriteLine("DIR: " + a.ToString());
            Console.WriteLine("MSK: " + Addresses.pointF(a.Mascara));
            Console.WriteLine("IDR: " + Addresses.pointF(a.IDRed));
            Console.WriteLine("IDB: " + Addresses.pointF(a.IDBroadcast));
            if(args.Length > 5)
            {                
                Console.WriteLine("Subneteo a /" + args[5]);
                foreach(Addresses s in Addresses.subnetWith(a,Convert.ToInt32(args[5])))
                {
                    Console.WriteLine("IDR: " + s.ToString() + ", IDB: " + Addresses.pointF(s.IDBroadcast));
                }
            }
        }
    }
}