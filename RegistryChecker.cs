using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;

namespace gep
{
    class RegistryChecker
    {
        public static int[] R;
        static byte[][] A; //arrays
        static bool flag = false;

        public const int RET = 0x80;//r : return r
        public const int JZ = 0x81; //v : if fl ip += v
        public const int JMP = 0x82; //v : ip += v 

        //modificators. R is number of register; V is value; A is number of register that has index in array z
        public const int RR = 0x00; //CMD|RR, rDst, rSrc
        public const int RA = 0x10; //CMD|RA, rDst, rIndex, vArray //vArray = 0..15 or -1 for R_array
        public const int RV = 0x20; //CMD|RV, rDst, vValue

        public const int ADD = 1;  //a1; a2 : a1 += a2
        public const int MUL = 2;  //a1; a2 : a1 *= a2
        public const int MOD = 3;  //a1; a2 : a1 %= a2
        public const int SUB = 4;  //a1; a2 : a1 -= a2
        public const int DIV = 9;  //a1; a2 : a1 /= a2
        public const int XOR = 10; //a1; a2 : a1 ^= a2
        public const int MOV = 5;  //a1; a2 : a1 = a2
        public const int LE = 7; //a1; a2 : fl = a1 < a2 
        public const int EQ = 8; //a1; a2 : fl = a1 == a2 */


        public RegistryChecker()
        {
            R = new int[256];
            A = new byte[16][];
            Random rnd = new Random();

            R[07] = 2214472;
            R[13] = 133247;
            R[14] = 106358;
            R[15] = rnd.Next();
            R[24] = 752379;
            R[52] = rnd.Next();
            R[55] = rnd.Next();
            R[66] = rnd.Next();
            R[81] = 272121; 
        }

        public void CalcDays()
        {
            DateTime dt = DateTime.Now;
            long curdate = dt.Ticks, firstdate = DateTime.MinValue.Ticks;
            long xr = XorValue();
            string keyname = @"CLSID\{8AF0B76D-52D5-4b6a-82EE-662078EE80FF}\InprocServer32";
            using (RegistryKey rk = Registry.ClassesRoot.OpenSubKey(keyname, true))
            {
                if (rk == null) //no key yet
                {
                    using(RegistryKey rk2 = Registry.ClassesRoot.CreateSubKey(keyname))
                        if (rk2 != null)
                            rk2.SetValue("", curdate ^ xr, RegistryValueKind.QWord);
                    firstdate = curdate;
                } 
                else                
                {
                    firstdate =(long) rk.GetValue("", (long)0);
                    if (firstdate == 0)
                    {
                        firstdate = curdate;
                        rk.SetValue("", curdate ^ xr, RegistryValueKind.QWord);
                    }
                    firstdate ^= xr;
                }
            }
            DateTime fd = new DateTime(firstdate);
            TimeSpan ts = dt - fd;
            R[13] = ts.Days;
            //MessageBox.Show(R[13].ToString());
        }

        /*
{0xe0, 0xbe, 0xf7, 0xcf, 0xba, 0xa4, 0x42, 0x50, 0xae, 0x67, 0x90, 0x01, 0x62, 0x22, 0x99, 0x91},
{0x67, 0xda, 0x9c, 0x87, 0x9c, 0x6a, 0x42, 0xd7, 0x9e, 0x16, 0x47, 0xf1, 0xd1, 0x73, 0xef, 0x45},
{0x3e, 0xfa, 0xba, 0x9a, 0x0d, 0xb4, 0x47, 0xd2, 0xbd, 0xfe, 0x12, 0xfe, 0x6a, 0xd9, 0xc9, 0x08},
{0x2d, 0x49, 0x8d, 0x40, 0x23, 0x7f, 0x45, 0x39, 0xb9, 0x63, 0x64, 0xcc, 0xaf, 0x41, 0xc6, 0xd7},
{0x46, 0xa9, 0xbe, 0x27, 0xa3, 0x8c, 0x4e, 0xa8, 0xb0, 0x58, 0x73, 0x89, 0x31, 0x1f, 0xf7, 0x43},
{0x28, 0xf6, 0xb4, 0x7e, 0x83, 0xb2, 0x4d, 0xc7, 0x8f, 0xc7, 0xca, 0x96, 0xc1, 0x1e, 0x00, 0x34},
{0x6d, 0xf7, 0xb4, 0xd6, 0xe6, 0x0c, 0x4c, 0x18, 0xb2, 0x7c, 0xec, 0xcb, 0x24, 0x78, 0x3f, 0x66},
{0x28, 0x74, 0xaa, 0x41, 0x31, 0x9c, 0x4d, 0x66, 0x83, 0x09, 0x92, 0x8f, 0xd6, 0x6c, 0xde, 0x5e},
{0x0a, 0xb8, 0xdb, 0x85, 0xfe, 0xd3, 0x41, 0xa7, 0x90, 0xcb, 0x64, 0xb2, 0xd7, 0x1e, 0xf7, 0x36},
{0x33, 0x4d, 0xfb, 0xdd, 0x90, 0x3c, 0x42, 0xb9, 0x9e, 0x8e, 0xbe, 0x4e, 0xf0, 0x64, 0x5d, 0xb8},
{0x3f, 0x0f, 0x06, 0x78, 0xd6, 0xdb, 0x48, 0x91, 0x81, 0x26, 0x03, 0xc6, 0xda, 0x47, 0xdb, 0x74},
{0x1a, 0xad, 0xd7, 0x73, 0x9f, 0x2f, 0x4d, 0xd0, 0x85, 0x56, 0x12, 0xe4, 0x8b, 0x27, 0x64, 0xcb},
{0xe6, 0x4a, 0x56, 0x46, 0x8c, 0x9a, 0x46, 0x99, 0x8f, 0x8c, 0x3a, 0x6c, 0xd4, 0x71, 0x56, 0xb5},
{0xbf, 0x49, 0xb8, 0x72, 0xc9, 0x5e, 0x46, 0x2d, 0xbb, 0x71, 0x93, 0x5c, 0x84, 0x0d, 0xbc, 0xdc},
{0xd8, 0x08, 0xdc, 0x4b, 0x39, 0x81, 0x4d, 0x88, 0xa6, 0x70, 0xaa, 0x52, 0xa2, 0xd5, 0xce, 0xd4},
{0x53, 0x81, 0x44, 0x99, 0x2c, 0x49, 0x42, 0x45, 0x8b, 0x23, 0x11, 0xc0, 0x72, 0xde, 0xd3, 0x6d},
         */

        /* R: 0..99 - params,  100-255 - registers
         * R[0] - [out] 1 if check was performed
         * R[1] - [out] 1 if registered, 0 if not
         * R[4] - [in] exe file size
         * R[5] - [in] email.Length
         * R[6] - [in] code.Length
         * R[13] - [in] days past first run
         * R[93] - [out] days left
         * A[0] - exefile
         * A[1] - email 16 chars
         * A[2] - code R[6] chars
         * invariants:
         * R[11] + R[24] + R[66] = 0
         * R[32] + R[07] + R[55] = 1
         * R[69] + R[81] + R[15] = 175
         * R[27] + R[14] + R[52] = 1 if registered, 0 if not
         */

        public bool CheckCode(string email, string code)
        {
            A[1] = new byte[16];
            A[2] = new byte[code.Length];
            R[5] = email.Length;
            R[6] = code.Length;
            byte[] buf;
            int sz = 0;
            //MessageBox.Show(Application.ExecutablePath);
            using (FileStream fs = new FileStream(Application.ExecutablePath, FileMode.Open, FileAccess.Read))
            {
                sz = (int)fs.Length;
                buf = new byte[sz];
                fs.Read(buf, 0, sz);
            }
            A[0] = buf;
            R[4] = sz;
            
            if (email.Length>0)
                for (int i = 0; i < 16; i++)
                    A[1][i] = (byte)email[i % email.Length];
            for (int i = 0; i < code.Length; i++)
                A[2][i] = (byte)code[i];


            /*R[11] = -(R[24] + R[66]);
            R[32] = -(R[07] + R[55] - 1);
            R[69] = -(R[81] + R[15] - 175);
            R[27] = -(R[14] + R[52] - 0);
            R[0] = 1;
            //R[1] = 1;//test
            if (R[13] <= 30)
                R[93] = 30 - R[13]; //days left*/
            Run(vmcode);
            /*int sum = 0;
            int codeval =  (hexc2int(A[2][0]) << 8) + (hexc2int(A[2][1]) << 4) + hexc2int(A[2][2]);
            for (int i = 0; i < 16; i++)
                sum += ((int)A[1][i] * (int)A[0][i])%256;*/
            //MessageBox.Show("check code: " + " vmret="+x/*.ToString("X")*/ + " vm.codeval="+R[25].ToString("X")
            //     + " vm.sum=" + R[20].ToString("X") + " sum="+sum);
            /*if (sum == codeval)
            {
                R[1] = 1;
                R[27] = -(R[14] + R[52] - 1);
            } */           
            return R[1] > 0;
        }

        static long XorValue()
        {
            long x = Run(xorval_code);
            //MessageBox.Show(x.ToString("X"));
            return (x<<32)+x;
        }

        /*int hexc2int(byte h)
        {
            if (h <= '9') return h - '0';
            else return h - 'A' + 10;
        }*/

        static int Run(int[] code)
        {
            int ip = 0, source = 0, cmdsz = 3;
            flag = false;
            while (ip >= 0 && ip < code.Length)
            {
                int cmd = code[ip];
                if ((cmd & 0x80) > 0)
                {
                    switch (cmd)
                    {
                        case RET: return R[code[ip + 1]];
                        case JZ:
                            if (!flag)
                                ip += code[ip + 1];
                            else
                                ip += 2;
                            break;
                        case JMP:
                            ip += code[ip + 1];
                            break;
                    }
                    continue;
                }

                switch (cmd & 0x30)
                {
                    case RR: source = R[code[ip + 2]]; cmdsz = 3; break;
                    case RV: source = code[ip + 2]; cmdsz = 3; break;
                    case RA:
                        int arr = code[ip + 3];
                        if (arr >= 0)
                            source = A[arr][R[code[ip + 2]]];
                        else
                            source = R[R[code[ip + 2]]];
                        cmdsz = 4;
                        break;
                }

                int di = code[ip + 1];
                switch (cmd & 0x0F)
                {
                    case ADD:
                        R[di] += source;
                        break;
                    case SUB:
                        R[di] -= source;
                        break;
                    case MOD:
                        R[di] %= source;
                        break;
                    case MUL:
                        R[di] *= source;
                        break;
                    case DIV:
                        R[di] /= source;
                        break;
                    case XOR:
                        R[di] ^= source;
                        break;
                    case MOV:
                        R[di] = source;
                        break;
                    case LE:
                        flag = (R[di] < source);
                        break;
                    case EQ:
                        flag = (R[di] == source);
                        break;
                    default:
                        ip -= 2;
                        break;
                }
                ip += cmdsz;

            }
            return -1;
        }

        readonly int[] vmcode = new int[] {
LE|RR, 144, 149,
JZ, 13,
MOV|RR, 150, 127,
SUB|RV, 150, 189,
MOV|RR, 134, 150,
JMP, 11,
MOV|RR, 150, 120,
ADD|RV, 150, 68,
MOV|RR, 140, 150,
MOV|RV, 150, 0,
MOV|RR, 151, 24,
ADD|RR, 151, 66,
SUB|RR, 150, 151,
MOV|RR, 11, 150,
MOV|RV, 150, 0,
MOV|RR, 151, 7,
ADD|RR, 151, 55,
SUB|RV, 151, 1,
SUB|RR, 150, 151,
MOV|RR, 32, 150,
LE|RR, 134, 129,
JZ, 13,
MOV|RR, 150, 149,
MUL|RV, 150, 92,
MOV|RR, 127, 150,
JMP, 11,
MOV|RR, 150, 122,
MUL|RV, 150, 205,
MOV|RR, 126, 150,
LE|RR, 133, 120,
JZ, 13,
MOV|RR, 150, 144,
ADD|RV, 150, 32,
MOV|RR, 142, 150,
JMP, 11,
MOV|RR, 150, 143,
MUL|RV, 150, 207,
MOV|RR, 132, 150,
LE|RR, 148, 120,
JZ, 13,
MOV|RR, 150, 141,
MUL|RV, 150, 72,
MOV|RR, 131, 150,
JMP, 11,
MOV|RR, 150, 127,
SUB|RV, 150, 129,
MOV|RR, 142, 150,
MOV|RV, 150, 0,
MOV|RR, 151, 81,
ADD|RR, 151, 15,
SUB|RV, 151, 175,
SUB|RR, 150, 151,
MOV|RR, 69, 150,
MOV|RV, 150, 0,
MOV|RR, 151, 14,
ADD|RR, 151, 52,
SUB|RV, 151, 0,
SUB|RR, 150, 151,
MOV|RR, 27, 150,
MOV|RV, 0, 1,
LE|RR, 143, 125,
JZ, 13,
MOV|RR, 150, 140,
MUL|RV, 150, 173,
MOV|RR, 148, 150,
JMP, 11,
MOV|RR, 150, 125,
ADD|RV, 150, 70,
MOV|RR, 127, 150,
LE|RR, 131, 132,
JZ, 13,
MOV|RR, 150, 147,
ADD|RV, 150, 92,
MOV|RR, 125, 150,
JMP, 11,
MOV|RR, 150, 145,
SUB|RV, 150, 219,
MOV|RR, 140, 150,
LE|RR, 145, 139,
JZ, 13,
MOV|RR, 150, 132,
ADD|RV, 150, 110,
MOV|RR, 139, 150,
JMP, 11,
MOV|RR, 150, 143,
MUL|RV, 150, 115,
MOV|RR, 147, 150,
LE|RV, 13, 31,
JZ, 13,
MOV|RV, 150, 30,
SUB|RR, 150, 13,
MOV|RR, 93, 150,
JMP, 2,
LE|RR, 145, 129,
JZ, 13,
MOV|RR, 150, 148,
SUB|RV, 150, 142,
MOV|RR, 136, 150,
JMP, 11,
MOV|RR, 150, 132,
SUB|RV, 150, 222,
MOV|RR, 143, 150,
EQ|RR, 130, 127,
JZ, 13,
MOV|RR, 150, 134,
ADD|RV, 150, 142,
MOV|RR, 135, 150,
JMP, 11,
MOV|RR, 150, 146,
ADD|RV, 150, 97,
MOV|RR, 148, 150,
LE|RR, 134, 130,
JZ, 13,
MOV|RR, 150, 124,
MUL|RV, 150, 255,
MOV|RR, 134, 150,
JMP, 11,
MOV|RR, 150, 145,
SUB|RV, 150, 194,
MOV|RR, 146, 150,
LE|RR, 149, 146,
JZ, 13,
MOV|RR, 150, 145,
SUB|RV, 150, 126,
MOV|RR, 132, 150,
JMP, 11,
MOV|RR, 150, 131,
MUL|RV, 150, 102,
MOV|RR, 136, 150,
LE|RV, 5, 1,
JZ, 9,
MOV|RV, 150, 0,
RET, 150,
JMP, 2,
LE|RV, 6, 3,
JZ, 9,
MOV|RV, 150, 0,
RET, 150,
JMP, 2,
EQ|RR, 149, 121,
JZ, 13,
MOV|RR, 150, 147,
SUB|RV, 150, 115,
MOV|RR, 133, 150,
JMP, 11,
MOV|RR, 150, 121,
SUB|RV, 150, 168,
MOV|RR, 141, 150,
LE|RR, 139, 148,
JZ, 13,
MOV|RR, 150, 126,
MUL|RV, 150, 27,
MOV|RR, 140, 150,
JMP, 11,
MOV|RR, 150, 127,
SUB|RV, 150, 118,
MOV|RR, 137, 150,
EQ|RR, 135, 138,
JZ, 13,
MOV|RR, 150, 149,
SUB|RV, 150, 44,
MOV|RR, 130, 150,
JMP, 11,
MOV|RR, 150, 121,
MUL|RV, 150, 62,
MOV|RR, 147, 150,
EQ|RR, 130, 125,
JZ, 13,
MOV|RR, 150, 147,
MUL|RV, 150, 110,
MOV|RR, 140, 150,
JMP, 11,
MOV|RR, 150, 121,
SUB|RV, 150, 25,
MOV|RR, 139, 150,
EQ|RR, 132, 126,
JZ, 13,
MOV|RR, 150, 133,
SUB|RV, 150, 43,
MOV|RR, 146, 150,
JMP, 11,
MOV|RR, 150, 123,
SUB|RV, 150, 22,
MOV|RR, 132, 150,
MOV|RV, 9, 0,
MOV|RV, 20, 1653,
LE|RR, 9, 4,
JZ, 11,
ADD|RA, 20, 9, 0,
ADD|RV, 9, 1,
JMP, -12,
EQ|RR, 137, 121,
JZ, 13,
MOV|RR, 150, 142,
SUB|RV, 150, 64,
MOV|RR, 142, 150,
JMP, 11,
MOV|RR, 150, 123,
SUB|RV, 150, 127,
MOV|RR, 135, 150,
EQ|RR, 138, 129,
JZ, 13,
MOV|RR, 150, 126,
MUL|RV, 150, 63,
MOV|RR, 146, 150,
JMP, 11,
MOV|RR, 150, 131,
ADD|RV, 150, 153,
MOV|RR, 134, 150,
EQ|RR, 138, 148,
JZ, 13,
MOV|RR, 150, 121,
ADD|RV, 150, 21,
MOV|RR, 124, 150,
JMP, 11,
MOV|RR, 150, 126,
MUL|RV, 150, 60,
MOV|RR, 142, 150,
EQ|RR, 135, 125,
JZ, 13,
MOV|RR, 150, 135,
ADD|RV, 150, 72,
MOV|RR, 140, 150,
JMP, 11,
MOV|RR, 150, 126,
SUB|RV, 150, 245,
MOV|RR, 137, 150,
EQ|RR, 145, 146,
JZ, 13,
MOV|RR, 150, 148,
SUB|RV, 150, 180,
MOV|RR, 124, 150,
JMP, 11,
MOV|RR, 150, 144,
ADD|RV, 150, 244,
MOV|RR, 121, 150,
MOD|RV, 20, 65536,
EQ|RV, 20, 0,
JZ, 104,
LE|RR, 121, 125,
JZ, 13,
MOV|RR, 150, 135,
ADD|RV, 150, 203,
MOV|RR, 128, 150,
JMP, 11,
MOV|RR, 150, 130,
SUB|RV, 150, 73,
MOV|RR, 128, 150,
LE|RR, 123, 132,
JZ, 13,
MOV|RR, 150, 132,
ADD|RV, 150, 116,
MOV|RR, 126, 150,
JMP, 11,
MOV|RR, 150, 125,
ADD|RV, 150, 175,
MOV|RR, 139, 150,
EQ|RR, 139, 128,
JZ, 13,
MOV|RR, 150, 148,
SUB|RV, 150, 55,
MOV|RR, 131, 150,
JMP, 11,
MOV|RR, 150, 136,
SUB|RV, 150, 2,
MOV|RR, 137, 150,
LE|RR, 125, 141,
JZ, 13,
MOV|RR, 150, 134,
MUL|RV, 150, 12,
MOV|RR, 133, 150,
JMP, 11,
MOV|RR, 150, 132,
MUL|RV, 150, 79,
MOV|RR, 142, 150,
JMP, 7,
MOV|RV, 150, 0,
RET, 150,
LE|RR, 124, 149,
JZ, 13,
MOV|RR, 150, 139,
MUL|RV, 150, 255,
MOV|RR, 147, 150,
JMP, 11,
MOV|RR, 150, 144,
ADD|RV, 150, 47,
MOV|RR, 125, 150,
EQ|RR, 142, 124,
JZ, 13,
MOV|RR, 150, 125,
MUL|RV, 150, 184,
MOV|RR, 146, 150,
JMP, 11,
MOV|RR, 150, 126,
MUL|RV, 150, 90,
MOV|RR, 146, 150,
MOV|RV, 9, 0,
LE|RR, 120, 131,
JZ, 13,
MOV|RR, 150, 125,
ADD|RV, 150, 11,
MOV|RR, 144, 150,
JMP, 11,
MOV|RR, 150, 144,
ADD|RV, 150, 83,
MOV|RR, 134, 150,
EQ|RR, 124, 141,
JZ, 13,
MOV|RR, 150, 126,
MUL|RV, 150, 142,
MOV|RR, 148, 150,
JMP, 11,
MOV|RR, 150, 127,
SUB|RV, 150, 143,
MOV|RR, 145, 150,
LE|RR, 127, 143,
JZ, 13,
MOV|RR, 150, 134,
SUB|RV, 150, 227,
MOV|RR, 127, 150,
JMP, 11,
MOV|RR, 150, 138,
ADD|RV, 150, 124,
MOV|RR, 121, 150,
EQ|RR, 122, 141,
JZ, 13,
MOV|RR, 150, 143,
SUB|RV, 150, 70,
MOV|RR, 140, 150,
JMP, 11,
MOV|RR, 150, 140,
ADD|RV, 150, 147,
MOV|RR, 120, 150,
LE|RR, 121, 127,
JZ, 13,
MOV|RR, 150, 133,
MUL|RV, 150, 149,
MOV|RR, 146, 150,
JMP, 11,
MOV|RR, 150, 139,
MUL|RV, 150, 28,
MOV|RR, 124, 150,
MOV|RA, 8, 9, 2,
LE|RV, 8, 60,
JZ, 10,
SUB|RV, 8, 48,
MOV|RR, 10, 8,
JMP, 8,
SUB|RV, 8, 55,
MOV|RR, 10, 8,
ADD|RV, 9, 1,
EQ|RR, 123, 129,
JZ, 13,
MOV|RR, 150, 147,
SUB|RV, 150, 160,
MOV|RR, 139, 150,
JMP, 11,
MOV|RR, 150, 148,
MUL|RV, 150, 157,
MOV|RR, 137, 150,
LE|RR, 148, 145,
JZ, 13,
MOV|RR, 150, 124,
ADD|RV, 150, 77,
MOV|RR, 147, 150,
JMP, 11,
MOV|RR, 150, 129,
ADD|RV, 150, 211,
MOV|RR, 125, 150,
EQ|RR, 120, 149,
JZ, 13,
MOV|RR, 150, 145,
ADD|RV, 150, 168,
MOV|RR, 125, 150,
JMP, 11,
MOV|RR, 150, 135,
ADD|RV, 150, 82,
MOV|RR, 141, 150,
MUL|RV, 10, 256,
MOV|RR, 25, 10,
MOV|RA, 8, 9, 2,
LE|RV, 8, 60,
JZ, 10,
SUB|RV, 8, 48,
MOV|RR, 10, 8,
JMP, 8,
SUB|RV, 8, 55,
MOV|RR, 10, 8,
ADD|RV, 9, 1,
MUL|RV, 10, 16,
ADD|RR, 25, 10,
MOV|RA, 8, 9, 2,
LE|RV, 8, 60,
JZ, 10,
SUB|RV, 8, 48,
MOV|RR, 10, 8,
JMP, 8,
SUB|RV, 8, 55,
MOV|RR, 10, 8,
ADD|RV, 9, 1,
ADD|RR, 25, 10,
EQ|RR, 123, 139,
JZ, 13,
MOV|RR, 150, 128,
SUB|RV, 150, 80,
MOV|RR, 126, 150,
JMP, 11,
MOV|RR, 150, 128,
MUL|RV, 150, 230,
MOV|RR, 125, 150,
MOV|RV, 100, 224,
MOV|RV, 101, 190,
LE|RR, 135, 128,
JZ, 13,
MOV|RR, 150, 147,
MUL|RV, 150, 5,
MOV|RR, 125, 150,
JMP, 11,
MOV|RR, 150, 147,
ADD|RV, 150, 91,
MOV|RR, 126, 150,
LE|RR, 131, 139,
JZ, 13,
MOV|RR, 150, 134,
MUL|RV, 150, 148,
MOV|RR, 137, 150,
JMP, 11,
MOV|RR, 150, 146,
SUB|RV, 150, 98,
MOV|RR, 124, 150,
MOV|RV, 102, 247,
MOV|RV, 103, 207,
MOV|RV, 104, 186,
EQ|RR, 140, 130,
JZ, 13,
MOV|RR, 150, 146,
ADD|RV, 150, 180,
MOV|RR, 124, 150,
JMP, 11,
MOV|RR, 150, 122,
SUB|RV, 150, 42,
MOV|RR, 148, 150,
LE|RR, 131, 139,
JZ, 13,
MOV|RR, 150, 131,
ADD|RV, 150, 173,
MOV|RR, 120, 150,
JMP, 11,
MOV|RR, 150, 132,
SUB|RV, 150, 220,
MOV|RR, 122, 150,
MOV|RV, 105, 164,
MOV|RV, 106, 66,
MOV|RV, 107, 80,
LE|RR, 143, 134,
JZ, 13,
MOV|RR, 150, 137,
SUB|RV, 150, 106,
MOV|RR, 143, 150,
JMP, 11,
MOV|RR, 150, 125,
ADD|RV, 150, 85,
MOV|RR, 130, 150,
MOV|RV, 108, 174,
MOV|RV, 109, 103,
EQ|RR, 131, 135,
JZ, 13,
MOV|RR, 150, 142,
SUB|RV, 150, 255,
MOV|RR, 134, 150,
JMP, 11,
MOV|RR, 150, 132,
MUL|RV, 150, 146,
MOV|RR, 134, 150,
EQ|RR, 124, 135,
JZ, 13,
MOV|RR, 150, 143,
MUL|RV, 150, 13,
MOV|RR, 120, 150,
JMP, 11,
MOV|RR, 150, 122,
ADD|RV, 150, 87,
MOV|RR, 129, 150,
LE|RR, 132, 145,
JZ, 13,
MOV|RR, 150, 122,
SUB|RV, 150, 147,
MOV|RR, 135, 150,
JMP, 11,
MOV|RR, 150, 140,
ADD|RV, 150, 135,
MOV|RR, 133, 150,
LE|RR, 147, 125,
JZ, 13,
MOV|RR, 150, 149,
ADD|RV, 150, 34,
MOV|RR, 149, 150,
JMP, 11,
MOV|RR, 150, 132,
MUL|RV, 150, 54,
MOV|RR, 140, 150,
LE|RR, 144, 124,
JZ, 13,
MOV|RR, 150, 129,
ADD|RV, 150, 27,
MOV|RR, 125, 150,
JMP, 11,
MOV|RR, 150, 122,
ADD|RV, 150, 239,
MOV|RR, 129, 150,
MOV|RV, 110, 144,
MOV|RV, 111, 1,
LE|RR, 128, 147,
JZ, 13,
MOV|RR, 150, 146,
SUB|RV, 150, 109,
MOV|RR, 140, 150,
JMP, 11,
MOV|RR, 150, 131,
ADD|RV, 150, 127,
MOV|RR, 137, 150,
LE|RR, 149, 145,
JZ, 13,
MOV|RR, 150, 122,
SUB|RV, 150, 138,
MOV|RR, 131, 150,
JMP, 11,
MOV|RR, 150, 133,
SUB|RV, 150, 138,
MOV|RR, 129, 150,
EQ|RR, 142, 127,
JZ, 13,
MOV|RR, 150, 145,
SUB|RV, 150, 29,
MOV|RR, 123, 150,
JMP, 11,
MOV|RR, 150, 141,
SUB|RV, 150, 60,
MOV|RR, 128, 150,
EQ|RR, 124, 127,
JZ, 13,
MOV|RR, 150, 130,
MUL|RV, 150, 97,
MOV|RR, 137, 150,
JMP, 11,
MOV|RR, 150, 147,
SUB|RV, 150, 106,
MOV|RR, 128, 150,
MOV|RV, 112, 98,
MOV|RV, 113, 34,
LE|RR, 124, 135,
JZ, 13,
MOV|RR, 150, 127,
SUB|RV, 150, 141,
MOV|RR, 136, 150,
JMP, 11,
MOV|RR, 150, 129,
MUL|RV, 150, 7,
MOV|RR, 143, 150,
MOV|RV, 114, 153,
MOV|RV, 115, 145,
MOV|RV, 20, 0,
MOV|RV, 9, 0,
LE|RR, 134, 127,
JZ, 13,
MOV|RR, 150, 131,
ADD|RV, 150, 7,
MOV|RR, 140, 150,
JMP, 11,
MOV|RR, 150, 122,
ADD|RV, 150, 236,
MOV|RR, 131, 150,
MOV|RV, 21, 100,
LE|RV, 9, 16,
JZ, 149,
MOV|RA, 150, 9, 1,
MUL|RA, 150, 21, -1,
MOD|RV, 150, 256,
ADD|RR, 20, 150,
ADD|RV, 9, 1,
EQ|RR, 120, 127,
JZ, 13,
MOV|RR, 150, 140,
ADD|RV, 150, 9,
MOV|RR, 130, 150,
JMP, 11,
MOV|RR, 150, 137,
ADD|RV, 150, 144,
MOV|RR, 137, 150,
EQ|RR, 134, 123,
JZ, 13,
MOV|RR, 150, 128,
MUL|RV, 150, 72,
MOV|RR, 146, 150,
JMP, 11,
MOV|RR, 150, 138,
MUL|RV, 150, 114,
MOV|RR, 146, 150,
EQ|RR, 128, 120,
JZ, 13,
MOV|RR, 150, 144,
MUL|RV, 150, 153,
MOV|RR, 122, 150,
JMP, 11,
MOV|RR, 150, 138,
SUB|RV, 150, 51,
MOV|RR, 131, 150,
EQ|RR, 124, 126,
JZ, 13,
MOV|RR, 150, 123,
ADD|RV, 150, 191,
MOV|RR, 146, 150,
JMP, 11,
MOV|RR, 150, 131,
ADD|RV, 150, 89,
MOV|RR, 135, 150,
EQ|RR, 122, 141,
JZ, 13,
MOV|RR, 150, 143,
ADD|RV, 150, 213,
MOV|RR, 139, 150,
JMP, 11,
MOV|RR, 150, 134,
MUL|RV, 150, 98,
MOV|RR, 139, 150,
ADD|RV, 21, 1,
JMP, -150,
LE|RR, 138, 147,
JZ, 13,
MOV|RR, 150, 145,
ADD|RV, 150, 14,
MOV|RR, 129, 150,
JMP, 11,
MOV|RR, 150, 145,
SUB|RV, 150, 116,
MOV|RR, 120, 150,
EQ|RR, 136, 122,
JZ, 13,
MOV|RR, 150, 142,
SUB|RV, 150, 150,
MOV|RR, 134, 150,
JMP, 11,
MOV|RR, 150, 128,
SUB|RV, 150, 179,
MOV|RR, 132, 150,
LE|RR, 141, 139,
JZ, 13,
MOV|RR, 150, 148,
ADD|RV, 150, 248,
MOV|RR, 133, 150,
JMP, 11,
MOV|RR, 150, 139,
ADD|RV, 150, 35,
MOV|RR, 148, 150,
EQ|RR, 122, 139,
JZ, 13,
MOV|RR, 150, 136,
MUL|RV, 150, 210,
MOV|RR, 145, 150,
JMP, 11,
MOV|RR, 150, 147,
ADD|RV, 150, 137,
MOV|RR, 147, 150,
EQ|RR, 20, 25,
JZ, 185,
EQ|RR, 122, 132,
JZ, 13,
MOV|RR, 150, 132,
SUB|RV, 150, 171,
MOV|RR, 147, 150,
JMP, 11,
MOV|RR, 150, 134,
SUB|RV, 150, 239,
MOV|RR, 138, 150,
LE|RR, 136, 139,
JZ, 13,
MOV|RR, 150, 140,
MUL|RV, 150, 24,
MOV|RR, 139, 150,
JMP, 11,
MOV|RR, 150, 120,
MUL|RV, 150, 56,
MOV|RR, 139, 150,
MOV|RV, 1, 1,
LE|RR, 145, 128,
JZ, 13,
MOV|RR, 150, 123,
MUL|RV, 150, 174,
MOV|RR, 132, 150,
JMP, 11,
MOV|RR, 150, 142,
SUB|RV, 150, 182,
MOV|RR, 139, 150,
EQ|RR, 145, 137,
JZ, 13,
MOV|RR, 150, 131,
SUB|RV, 150, 168,
MOV|RR, 124, 150,
JMP, 11,
MOV|RR, 150, 124,
ADD|RV, 150, 96,
MOV|RR, 141, 150,
EQ|RR, 139, 130,
JZ, 13,
MOV|RR, 150, 141,
MUL|RV, 150, 40,
MOV|RR, 136, 150,
JMP, 11,
MOV|RR, 150, 129,
ADD|RV, 150, 120,
MOV|RR, 140, 150,
EQ|RR, 142, 135,
JZ, 13,
MOV|RR, 150, 128,
SUB|RV, 150, 224,
MOV|RR, 132, 150,
JMP, 11,
MOV|RR, 150, 120,
SUB|RV, 150, 91,
MOV|RR, 137, 150,
EQ|RR, 130, 122,
JZ, 13,
MOV|RR, 150, 144,
SUB|RV, 150, 215,
MOV|RR, 121, 150,
JMP, 11,
MOV|RR, 150, 136,
MUL|RV, 150, 217,
MOV|RR, 146, 150,
ADD|RV, 27, 1,
JMP, 27,
LE|RR, 131, 140,
JZ, 13,
MOV|RR, 150, 147,
ADD|RV, 150, 99,
MOV|RR, 143, 150,
JMP, 11,
MOV|RR, 150, 129,
MUL|RV, 150, 19,
MOV|RR, 137, 150,
EQ|RR, 131, 122,
JZ, 13,
MOV|RR, 150, 124,
MUL|RV, 150, 38,
MOV|RR, 130, 150,
JMP, 11,
MOV|RR, 150, 136,
SUB|RV, 150, 247,
MOV|RR, 121, 150,
EQ|RR, 146, 121,
JZ, 13,
MOV|RR, 150, 143,
SUB|RV, 150, 163,
MOV|RR, 131, 150,
JMP, 11,
MOV|RR, 150, 134,
MUL|RV, 150, 61,
MOV|RR, 121, 150,
RET, 1
        };

        static readonly int[] xorval_code = new int[] {
LE|RR, 138, 120,
JZ, 13,
MOV|RR, 150, 139,
SUB|RV, 150, 90,
MOV|RR, 124, 150,
JMP, 11,
MOV|RR, 150, 121,
MUL|RV, 150, 12,
MOV|RR, 123, 150,
EQ|RR, 125, 144,
JZ, 13,
MOV|RR, 150, 125,
MUL|RV, 150, 143,
MOV|RR, 143, 150,
JMP, 11,
MOV|RR, 150, 143,
SUB|RV, 150, 166,
MOV|RR, 141, 150,
EQ|RR, 123, 148,
JZ, 13,
MOV|RR, 150, 127,
ADD|RV, 150, 161,
MOV|RR, 130, 150,
JMP, 11,
MOV|RR, 150, 136,
MUL|RV, 150, 254,
MOV|RR, 138, 150,
MOV|RV, 31, 1783,
ADD|RR, 31, 31,
LE|RR, 126, 134,
JZ, 13,
MOV|RR, 150, 134,
MUL|RV, 150, 3,
MOV|RR, 134, 150,
JMP, 11,
MOV|RR, 150, 140,
SUB|RV, 150, 243,
MOV|RR, 129, 150,
LE|RR, 137, 142,
JZ, 13,
MOV|RR, 150, 128,
ADD|RV, 150, 92,
MOV|RR, 121, 150,
JMP, 11,
MOV|RR, 150, 134,
SUB|RV, 150, 199,
MOV|RR, 124, 150,
MOV|RR, 32, 31,
LE|RR, 139, 148,
JZ, 13,
MOV|RR, 150, 136,
ADD|RV, 150, 115,
MOV|RR, 134, 150,
JMP, 11,
MOV|RR, 150, 134,
SUB|RV, 150, 237,
MOV|RR, 149, 150,
EQ|RR, 123, 122,
JZ, 13,
MOV|RR, 150, 132,
ADD|RV, 150, 225,
MOV|RR, 147, 150,
JMP, 11,
MOV|RR, 150, 138,
SUB|RV, 150, 241,
MOV|RR, 138, 150,
EQ|RR, 122, 147,
JZ, 13,
MOV|RR, 150, 149,
MUL|RV, 150, 157,
MOV|RR, 125, 150,
JMP, 11,
MOV|RR, 150, 125,
ADD|RV, 150, 103,
MOV|RR, 133, 150,
EQ|RR, 125, 144,
JZ, 13,
MOV|RR, 150, 146,
ADD|RV, 150, 186,
MOV|RR, 124, 150,
JMP, 11,
MOV|RR, 150, 148,
MUL|RV, 150, 128,
MOV|RR, 141, 150,
MOV|RR, 150, 31,
MUL|RV, 150, 4096,
ADD|RR, 150, 32,
RET, 150
        };

    }
}
