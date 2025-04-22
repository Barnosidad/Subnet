namespace subnet
{
    class Program
    {
        public static void Main(string [] args)
        {
            Console.WriteLine("Hola direccion");
            Addresses d = new Addresses(192,168,0,10,24);
            Console.WriteLine(d.ToString());
            Console.WriteLine("Mascara: " + Addresses.pointF(d.Mascara));
            Console.WriteLine("IDR: " + Addresses.pointF(d.IDRed));
            Console.WriteLine("IDB: " + Addresses.pointF(d.IDBroadcast));
            Console.WriteLine("Subneteo a /30");
            foreach (Addresses a in Addresses.subnetWith(d,30))
            {
                Console.WriteLine(a.ToString());
            }
            Console.WriteLine("Todas las direcciones validas del primer segmento");
            foreach (Addresses b in Addresses.rangeBetween(Addresses.subnetWith(d,30).First()))
            {
                Console.WriteLine(b.ToString());
            }
        }
    }
}