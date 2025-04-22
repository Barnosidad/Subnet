// Gonzalez Hernandez Diego
// Subneteos usando bit shifting y algunas otras locuras simples

// Explicacion de shifteo
// El shifteo de bits mueve lost bits de una variable a izquierda o derecha n veces
// Sintaxis: (V) >> (N) o (V) << (N) ; este puede ser igualado a la vez
// Tambien hay que notar que hay varios tipos de shifting, en este caso cuando los valores salen del limite de bits
// de la variable a tratar se eliminan esos valores y la variable se va haciendo cero; ejemplo de 8 bits
// |1|0|1|1|0|0|1|0| << (3) = |1|0|0|1|0|0|0|0|
// Al momento de recuperar los valores y castearlos se toma siempre el valor del bit menos significativo hacia la izquierda
// Ejemplo de 8 bits casteado a 4 bits
// (4 bits) |1|0|1|1|0|0|1|0| = |0|0|1|0|
// Algo util a la hora de trabajar con las direcciones

//  Explicacion de la Mascara de Subred
// La mascara que inicialmente es cero en hexadecimal pasa a negarse para encender todos los bits, despues de eso hacemos un
// shifting a la derecha n veces siendo n la porcion de host, eliminando esos bits y regresando el shift a la izquierda esos
// mismos n veces, asi obtenemos la mascara de subred, ejemplo:
// Para una /30 la porcion de host es 2, hacecmos dos shifting que se anulen
// |1|1|1|1|1|1|1|1|...|1|1|1|1|1|1|1|1| >>= (2) = |0|0|1|1|1|1|1|1|...|1|1|1|1|1|1|1|1|
// |0|0|1|1|1|1|1|1|...|1|1|1|1|1|1|1|1| <<= (2) = |1|1|1|1|1|1|1|1|...|1|1|1|1|1|1|0|0|
// Lo que al final nos dejara un entero de 32 bits que faicilmente se puede transformar a formato de punto.

// El ID de Red solo es un and'ing de la direccion IPV4 y su Mascara de subred (bit por bit) 

//  Explicacion de la ID de Broadcast
// La IDB que inicialmente es cero en hexadecimal pasa a negarse para encender todos los bits, despues de eso hacemos un
// shifting a la izquierda n veces siendo n la porcion de red, eliminando esos bits y regresando el shift a la derecha esos
// mismos n veces, despues sumamos el valor binario de la porcion de host (teniendo en cuenta que siempre sera menor o igual que esa cantidad de bits), ejemplo:
// La direccion es 192.168.1.136, para una /30 la porcion de host es 2, hacecmos dos shifting que se anulen
// |1|1|1|1|1|1|1|1|...|1|1|1|1|1|1|1|1| <<= (30) = |1|1|0|0|0|0|0|0|...|0|0|0|0|0|0|0|0|
// |1|1|0|0|0|0|0|0|...|0|0|0|0|0|0|0|0| >>= (30) = |0|0|0|0|0|0|0|0|...|0|0|0|0|0|0|1|1|
// Ahora sumamos el valor binario de la porcion de host
// |0|0|0|0|0|0|0|0|...|0|0|0|0|0|0|1|1| += |1|1|0|0|0|0|0|0|...|1|0|0|0|1|0|0|0| = |1|1|0|0|0|0|0|0|...|1|0|0|0|1|0|1|1|
// Lo que al final nos dejara un entero de 32 bits que faicilmente se puede transformar a formato de punto.

using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography.X509Certificates;

namespace subnet
{
    // Clase de direcciones para tener una abstraccion de lo que es una direccion
    public class Addresses
    {
        // Ahora todo tiene que basarse en la direccion uint de 32 bits y no en los arreglos de bytes
        // Estos son 3 tipos de constructores
        // El primero necesita una direccion proporcionada octeto por octeto de izquierda a derecha y su slash, pero usando bytes (8 bits)
        public Addresses(byte _O1, byte _O2, byte _O3, byte _O4, int _slash)
        {
            // Inicializo estas direcciones en CERO
            DirB = 0x00000000;
            Msk = 0x00000000;
            IDR = 0x00000000;
            IDB = 0x00000000;
            // La porcion de red es el slash, su complemento es la porcion de host
            int _NP = _slash, _HP = dirLenght - _slash;
            // Si la suma de ambos es mayor que el tamaño maximo de una direccion IPV4 no procede
            if(_NP + _HP > dirLenght) throw new ArgumentException("No puedes asignar esas porciones. ");
            // Si alguna de estas es menor que uno tampoco procedemos
            if(_NP < 1 || _HP < 1) throw new ArgumentException("Alguna de las dos porciones es muy pequena. ");
            // Recurre a la explicacion de shifteo
            DirB += _O1;
            DirB <<= 8;
            DirB += _O2;
            DirB <<= 8;
            DirB += _O3;
            DirB <<= 8;
            DirB += _O4;
            // Asigna la porcion de red y de host
            NP = _NP;
            HP = _HP;
            // Genero automaticamente de una direccion su mascara, su IDB y IDR
            GenerateMask();
            GenerateIDR();
            GenerateIDB();
        }
        // El segundo necesita una direccion proporcionada octeto por octeto de izquierda a derecha y su slash, pero usando enteros de 64 bits
        public Addresses(int _O1, int _O2, int _O3, int _O4, int _slash)
        {
            // Inicializo estas direcciones en CERO
            DirB = 0x00000000;
            Msk = 0x00000000;
            IDR = 0x00000000;
            IDB = 0x00000000;
            // La porcion de red es el slash, su complemento es la porcion de host
            int _NP = _slash, _HP = dirLenght - _slash;
            // Si la suma de ambos es mayor que el tamaño maximo de una direccion IPV4 no procede
            if(_NP + _HP > dirLenght) throw new ArgumentException("No puedes asignar esas porciones. ");
            // Si alguna de estas es menor que uno tampoco procedemos
            if(_NP < 1 || _HP < 1) throw new ArgumentException("Alguna de las dos porciones es muy pequena. ");
            if(_O1 > 255 || _O2 > 255 || _O3 > 255 || _O4 > 255) throw new ArgumentException("No puedes exceder el valor de un octeto. ");
            // Recurre a la explicacion de shifteo
            DirB += (byte) _O1;
            DirB <<= 8;
            DirB += (byte) _O2;
            DirB <<= 8;
            DirB += (byte) _O3;
            DirB <<= 8;
            DirB += (byte) _O4;
            // Asigna la porcion de red y de host
            NP = _NP;
            HP = _HP;
            // Genero automaticamente de una direccion su mascara, su IDB y IDR
            GenerateMask();
            GenerateIDR();
            GenerateIDB();
        }
        // La tercera requiere una direccion escrita en formato binario de 32 bits junto con el slash
        public Addresses(uint _Direccion, int _slash)
        {
            // Inicializo estas direcciones en CERO
            DirB = 0x00000000;
            Msk = 0x00000000;
            IDR = 0x00000000;
            IDB = 0x00000000;
            // La porcion de red es el slash, su complemento es la porcion de host
            int _NP = _slash, _HP = dirLenght - _slash;
            // Si la suma de ambos es mayor que el tamaño maximo de una direccion IPV4 no procede
            if(_NP + _HP > dirLenght) throw new ArgumentException("No puedes asignar esas porciones. ");
            // Si alguna de estas es menor que uno tampoco procedemos
            if(_NP < 1 || _HP < 1) throw new ArgumentException("Alguna de las dos porciones es muy pequena. ");
            // Solo almaceno la direccion, deberia ser valida si o si
            DirB = _Direccion;
            // Asigna la porcion de red y de host
            NP = _NP;
            HP = _HP;
            // Genero automaticamente de una direccion su mascara, su IDB y IDR
            GenerateMask();
            GenerateIDR();
            GenerateIDB();
        }
        // La direccion IPV4 almacenada en un entero sin signo de 32 bits
        private uint DirB;
        // La mascara de subred almacenada en un entero sin signo de 32 bits
        private uint Msk;
        // La ID de red almacenada en un entero sin signo de 32 bits
        private uint IDR;
        // La ID de broadcast almacenada en un entero sin signo de 32 bits
        private uint IDB;
        // Las Porciones de red y host almacenados en enteros de 64 bits (me parece ridiculo usar 5 bits solamente)
        private int NP, HP;
        // El tamaño de una direccion IPV4 normal
        public const int dirLenght = 32;
        // El getter y setter de la propiedad Addresses IP
        public uint DireccionIP
        {
            get => DirB;
            set => DirB = value;
        }
        // El getter de la propiedad Primer Octeto (Recurre a la explicacion de shifting)
        public byte PO
        {
            get => (byte) (DirB >> 24);
        }
        // El getter de la propiedad Segundo Octeto (Recurre a la explicacion de shifting)
        public byte SO
        {
            get => (byte) (DirB >> 16);
        }
        // El getter de la propiedad Tercer Octeto (Recurre a la explicacion de shifting)
        public byte TO
        {
            get => (byte) (DirB >> 8);
        }
        // El getter de la propiedad Cuarto Octeto (Recurre a la explicacion de shifting)
        public byte CO
        {
            get => (byte) DirB;
        }
        // El getter de la propiedad Mascara
        public uint Mascara
        {
            get => Msk;
            set => Msk = value;
        }
        // El getter de la propiedad de la ID de Red
        public uint IDRed
        {
            get => IDR;
            set => IDR = value;
        }
        // El getter de la propiedad de la ID de Broadcast
        public uint IDBroadcast
        {
            get => IDB;
            set => IDB = value;
        }
        // El getter de la propiedad de la Porcion de Red
        public int NetPortion
        {
            get => NP;
        }
        // El getter de la propiedad de la Porcion de Host
        public int HostPortion
        {
            get => HP;
        }
        // Calculo automaticamente la mascara de subred, consulta la explicacion.
        private void GenerateMask()
        {
            Msk = ~Msk;
            // Vueltas de carro
            Msk >>= HP;
            Msk <<= HP;
        }
        // Calculo automaticamente la ID de Red, consulta la explicacion.
        private void GenerateIDR()
        {
            IDR = Msk & DirB; 
        }
        // Calculo automaticamente la ID de Broadcast, consulta la explicacion.
        private void GenerateIDB()
        {
            IDB = ~IDB;
            // Vueltas de carro
            IDB <<= NP;
            IDB >>= NP;
            IDB += IDR;
        }
        // Sobreescribimos el ToString para que nos devuelva la direccion junto con su slash
        public override string ToString() => $"{PO}.{SO}.{TO}.{CO}/{NP}";
        // Este metodo transforma cualquier entero sin signo de 32 bits al formato de punto
        public static string pointF(uint formato)
        {
            string dir;
            dir =+ (byte) (formato >> 24) + "." + (byte) (formato >> 16) + "." + (byte) (formato >> 8) + "." + (byte) formato;
            return dir;
        }
        // Regresa el slash en formato de cadena para concatenaciones
        public string Slash
        {
            get => Convert.ToString("/" + NP);
        }
        // Subneteo con una mascara no variable y no recursivo, anadir el numero magico en la posicion del ultimo bit de la nueva mascara
        public static List<Addresses> subnetWith(Addresses Dir, int slash)
        {
            if(slash <= Dir.NP) throw new ArgumentException("El slash debe ser mayor a la porcion de RED de la MASCARA. ", nameof(slash));
            // Porcion de host y red
            int HP = dirLenght - slash, NP = slash - Dir.NetPortion;
            // Numero de redes y de direcciones
            int n = (int) Math.Pow(2, NP), m = (int) Math.Pow(2, HP) - 2;
            /* Numero magico
            int mn = 0;
            if(slash / 8 == 0) mn = 1;
            else for(int i = 0; i < (slash % 8); i++) mn = (int) Math.Pow(2, 7 - i);
            */
            // Addresses nuevas (una lista de direcciones tal vez)
            Addresses aux = new Addresses(Dir.IDRed, slash);
            List<Addresses> idSubnets = new List<Addresses>();
            // Calcular el id de red nuevo Y Anadir a la lista a regresar
            // Shifteo a la derecha hasta llegar al slash nuevo
            for(int i = 0; i < n; i++)
            {
                // desplazo a la derecha hasta llegar al nuevo slash (nueva porcion de host)
                aux.DireccionIP >>= HP;
                // sumo de uno en uno, o no
                if(i!=0) aux.DireccionIP += 1;
                else aux.DireccionIP += 0;
                // regreso el carrito
                aux.DireccionIP <<= HP;
                // asigno esta direccion
                idSubnets.Add(new Addresses(aux.DireccionIP, slash));
            }
            return idSubnets;
        }
        // El rango de direcciones completo de un ID Red
        public static List<Addresses> rangeBetween(Addresses Dir)
        {
            // Tendria que sumar a la ID de Red los bits de la porcion de host
            Addresses aux = new Addresses(Dir.DireccionIP, Dir.NetPortion);
            List<Addresses> addresses = new List<Addresses>();
            int m = (int) Math.Pow(2, Dir.HostPortion);
            for(int i = 1; i < (m - 1); i++)
            {
                // sumo de uno en uno
                aux.IDRed += 1;
                // asigno esta direccion
                addresses.Add(new Addresses(aux.IDRed, Dir.NetPortion));
            }
            return addresses;
        }
    }
}