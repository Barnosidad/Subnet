using System.Text.RegularExpressions;

namespace subnet
{
    class Program
    {
        public static void Main(string [] args)
        {
            Console.WriteLine("El numero de argumentos es: " + args.Length);
            if(args.Length == 0)
            {
                Console.WriteLine("\t\t\t .: Subnetting program :.");
                Console.WriteLine("\t\t Developed By Diego Gonzalez Hernandez.");
                Console.WriteLine("\t\t ITQ; Ingenieria en sistemas computacionales.");
                Console.WriteLine("\t\t\t .: Usage :.");
                Console.WriteLine("\t\t Ingresa el ID de RED base con sus 4 octetos separados por espacios y no por puntos.");
                Console.WriteLine("\t\t Ejemplo: 10.0.0.0 => 10 0 0 0");
                Console.WriteLine("\t\t La mascara (slash, no numerica) la pones justo despues separada por un espacio tambien.");
                Console.WriteLine("\t\t En caso de solo poner la mascara se te dara informacion asociada a esa direccion.");
                Console.WriteLine("\t\t Ejemplo: 10.0.0.0 /16 => 10 0 0 0 16");
                Console.WriteLine("\t\t Para subnetear agregas la mascara de destino (slash, no numerica) tambien separada con un espacio.");
                Console.WriteLine("\t\t Ejemplo: 10.0.0.0 /16 a /22 => 10 0 0 0 16 22");
            }
            else if(args.Length > 0 && args.Length < 5)
            {
                Console.WriteLine("\t\t\t .: Subnetting program :.");
                Console.WriteLine("\t\t Para mas informacion usa el comando Subnet sin argumentos!");
            }
            else
            {
                Addresses a = new Addresses(Convert.ToInt32(args[0]), Convert.ToInt32(args[1]), Convert.ToInt32(args[2]), Convert.ToInt32(args[3]), Convert.ToInt32(args[4]));
                if(args.Length < 6) Console.WriteLine("\t\t .: Only for information purpose :.");
                Console.WriteLine("DIR: " + a.ToString());
                Console.WriteLine("MSK: " + Addresses.pointF(a.Mascara));
                Console.WriteLine("IDR: " + Addresses.pointF(a.IDRed));
                Console.WriteLine("IDB: " + Addresses.pointF(a.IDBroadcast));
                if(args.Length > 5)
                {      
                    if(args[5] != "-l")
                    {    
                        Console.WriteLine("Subneteo a /" + args[5]);
                        foreach(Addresses s in Addresses.subnetWith(a,Convert.ToInt32(args[5])))
                        {
                            Console.WriteLine("IDR: " + s.ToString() + ", IDB: " + Addresses.pointF(s.IDBroadcast));
                        }
                    }
                    else
                    {
                        List<int> devices = new List<int>();
                        for(int i = 6; i < args.Length; i++)
                            devices.Add(Convert.ToInt32(args[i]));
                        devices.ForEach(x => Console.WriteLine(x));
                        // ! Aqui es la parte que seria mejor hacer recursividad
                        /*
                        Console.WriteLine("El slash mas factible es: " + slash_size(devices[0]));
                        if(Convert.ToInt32(args[4]) >= (32 - slash_size(devices[0]))) Console.WriteLine("No es posible hacer el subneteo!");
                        Console.WriteLine("Subneteo a /" + (32 - slash_size(devices[0])));
                        foreach(Addresses s in Addresses.subnetWith(a, Convert.ToInt32(32 - slash_size(devices[0]))))
                        {
                            Console.WriteLine("IDR: " + s.ToString() + ", IDB: " + Addresses.pointF(s.IDBroadcast));
                        }
                        */
                        subnet_devices(a, Convert.ToInt32(args[4]), devices);
                    }
                }
            }
        }
        public static int slash_size(int devices)
        {
            int i;
            for(i = 1; i < 32; i++)
                if(Math.Pow(2,i) - 2 >= devices) break;
            return i;
        }
        public static void subnet_devices(Addresses a, int slash, List<int> devices)
        {
            int count = 0;
            Console.WriteLine("El slash mas factible es: " + slash_size(devices[0]));
            if(Convert.ToInt32(slash) >= (32 - slash_size(devices[0]))) throw new ArgumentException("No es posible hacer ese subneteo!");
            Console.WriteLine("Subneteo a /" + (32 - slash_size(devices[0])));
            List<Addresses> subnet = Addresses.subnetWith(a, Convert.ToInt32(32 - slash_size(devices[0])));
            Console.WriteLine("Numero de redes: " + subnet.Count);
            foreach(Addresses s in subnet)
            {
                Console.WriteLine("#" + (count++) + " => IDR: " + s.ToString() + ", IDB: " + Addresses.pointF(s.IDBroadcast));
            }
            devices.RemoveAt(0);
            if(devices.Count >= 1)
                subnet_devices(subnet.ElementAt(subnet.Count - 2), subnet[0].NetPortion, devices);
            else return;
        }
    }
}