using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace gep
{
    struct Pair<A, B>
    {
        public A fst;
        public B snd;

        public Pair(A x, B y) { fst = x; snd = y; }
    }

    struct Unit { }

    delegate Thunk Thunk();
    delegate Thunk Kont<T>(T x);

    class Res<T> : Exception
    {
        public T res;
        public Res(T x) { res = x; }
    }

    class Sum<A, B>
    {
    }

    class Left<A, B> : Sum<A, B>
    {
        public A a;
        public Left(A x) { a = x; }
    }
    class Right<A, B> : Sum<A, B>
    {
        public B b;
        public Right(B x) { b = x; }
    }

    delegate Thunk RF<T>(T x, Kont<T> f);

    class UnRec<T>
    {
        RF<T> rf;
        public UnRec(RF<T> recfun) { rf = recfun; }
        public Thunk g(T x) { return rf(x, g); }
    }

    class RegistryChecker
    {
        public static int[] R;
        EventWaitHandle evtDone;
        Thread thr;

        public RegistryChecker()
        {
            R = new int[256];
            Random rnd = new Random();

            evtDone = new EventWaitHandle(false, EventResetMode.ManualReset);

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
            var di = new DirectoryInfo(Application.StartupPath);
            var fd = di.CreationTime;
            TimeSpan ts = dt - fd;
            R[05] = 1858; // just some noise
            R[08] = 2304;
            R[12] = 14;
            R[13] = ts.Days;
            R[16] = 30;
            R[48] = 16384;
        }

        static Pair<A, B> pair<A, B>(A a, B b)
        {
            Pair<A, B> p;
            p.fst = a; p.snd = b;
            return p;
        }

        static Unit unit;

        static Thunk run<T>(Kont<T> f, T x)
        {
            return f(x);
        }

        static Kont<T> unrec<T>(RF<T> rf)
        {
            return new UnRec<T>(rf).g;
        }

        static Sum<A, B> left<A, B>(A a)
        {
            return new Left<A, B>(a);
        }
        static Sum<A, B> right<A, B>(B b)
        {
            return new Right<A, B>(b);
        }

        static Thunk match<A, B>(Sum<A, B> e, Kont<A> fa, Kont<B> fb)
        {
            var l = e as Left<A, B>;
            if (l != null) return () => fa(l.a);
            var r = e as Right<A, B>;
            if (r != null) return () => fb(r.b);
            throw new Exception("bad Sum value");
        }

        static Sum<Unit, Unit> eql(int a, int b)
        {
            if (a == b) return right<Unit, Unit>(unit);
            else return left<Unit, Unit>(unit);
        }

        static Sum<Unit, Unit> less(int a, int b)
        {
            if (a < b) return right<Unit, Unit>(unit);
            else return left<Unit, Unit>(unit);
        }

        static int[] set(int[] a, int i, int v)
        {
            a[i] = v;
            return a;
        }

        static Kont<Kont<T>> lazy<T>(T x) { return k => k(x); }

        static Kont<Kont<int[]>> ofString(string s)
        {
            var m = new int[s.Length];
            for (int i = 0; i < s.Length; i++)
                m[i] = (int)s[i];
            return lazy(m);
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

        public void EnsureInited()
        {
            if (R[0] == 0)
            {
                if (thr == null) throw new Exception("EnsureInited called before thread created!");
                evtDone.WaitOne();
            }
        }

        public void StartChecking(string emailstr, string codestr)
        {
            if (thr != null) return;
            thr = new Thread(RCThreadProc);
            thr.Start(pair(emailstr, codestr));
        }

        void RCThreadProc(Object o)
        {
            var ec = (Pair<string, string>)o;
            CheckCode(ec.fst, ec.snd);                       
        }

        public bool CheckCode(string emailstr, string codestr)
        {
            /*R[11] = -(R[24] + R[66]);
            R[32] = -(R[07] + R[55] - 1);
            R[69] = -(R[81] + R[15] - 175);
            R[27] = -(R[14] + R[52] - 0);
            R[0] = 1;
            //R[1] = 1;//test
            if (R[13] <= 30)
                R[93] = 30 - R[13]; //days left*/

            var r = lazy(R);
            var elen = lazy(emailstr.Length);
            var codelen = lazy(codestr.Length);
            var email = ofString(emailstr);
            var code = ofString(codestr);

            Thunk fmain = () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int[]>>>>(x0 => () => run<Pair<Kont<Kont<int[]>>, Kont<int[]>>>(x0, pair<Kont<Kont<int[]>>, Kont<int[]>>(k1 => () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int[]>>>>(x2 => () => run<Pair<Kont<Kont<int[]>>, Kont<int[]>>>(x2, pair<Kont<Kont<int[]>>, Kont<int[]>>(k3 => () => run<Kont<int[]>>(r, m44 => () => run<int>(i45 => () => run<int>(a47 => () => run<Kont<int[]>>(r, m53 => () => run<int>(i54 => () => run<int>(a49 => () => run<Kont<int[]>>(r, m51 => () => run<int>(i52 => () => run<int>(b50 => () => run<int>(b48 => () => run<int>(v46 => () => run<int[]>(m31 => () => run<int>(i32 => () => run<int>(a34 => () => run<Kont<int[]>>(r, m42 => () => run<int>(i43 => () => run<int>(a38 => () => run<Kont<int[]>>(r, m40 => () => run<int>(i41 => () => run<int>(b39 => () => run<int>(a36 => () => run<int>(b37 => () => run<int>(b35 => () => run<int>(v33 => () => run<int[]>(m18 => () => run<int>(i19 => () => run<int>(a21 => () => run<Kont<int[]>>(r, m29 => () => run<int>(i30 => () => run<int>(a25 => () => run<Kont<int[]>>(r, m27 => () => run<int>(i28 => () => run<int>(b26 => () => run<int>(a23 => () => run<int>(b24 => () => run<int>(b22 => () => run<int>(v20 => () => run<int[]>(m7 => () => run<int>(i8 => () => run<int>(a10 => () => run<Kont<int[]>>(r, m16 => () => run<int>(i17 => () => run<int>(a12 => () => run<Kont<int[]>>(r, m14 => () => run<int>(i15 => () => run<int>(b13 => () => run<int>(b11 => () => run<int>(v9 => () => run<int[]>(m4 => () => run<int>(i5 => () => run<int>(v6 => () => run<int[]>(k3, set(m4, i5, v6)), 1), 0), set(m7, i8, v9)), (a10 - b11)), (a12 + b13)), m14[i15]), 52)), m16[i17]), 14)), 0), 27), set(m18, i19, v20)), (a21 - b22)), (a23 - b24)), 175), (a25 + b26)), m27[i28]), 15)), m29[i30]), 81)), 0), 69), set(m31, i32, v33)), (a34 - b35)), (a36 - b37)), 1), (a38 + b39)), m40[i41]), 55)), m42[i43]), 7)), 0), 32), set(m44, i45, v46)), (a47 - b48)), (a49 + b50)), m51[i52]), 66)), m53[i54]), 24)), 0), 11)), k1)), w55 => () => run<Kont<int[]>>(w55.fst, m68 => () => run<int>(i69 => () => run<int>(a66 => () => run<int>(b67 => () => run<Sum<Unit, Unit>>(z58 => match<Unit, Unit>(z58, u56 => () => run<Kont<int[]>>(w55.fst, w55.snd), u57 => () => run<Kont<int[]>>(w55.fst, m59 => () => run<int>(i60 => () => run<int>(a62 => () => run<Kont<int[]>>(w55.fst, m64 => () => run<int>(i65 => () => run<int>(b63 => () => run<int>(v61 => () => run<int[]>(w55.snd, set(m59, i60, v61)), (a62 - b63)), m64[i65]), 13)), 30), 93))), less(a66, b67)), 31), m68[i69]), 13))), x => { throw new Res<int[]>(x); })), w70 => () => run<Kont<int>>(elen, a217 => () => run<int>(b218 => () => run<Sum<Unit, Unit>>(z73 => match<Unit, Unit>(z73, u71 => () => run<Kont<int>>(codelen, a215 => () => run<int>(b216 => () => run<Sum<Unit, Unit>>(z76 => match<Unit, Unit>(z76, u74 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int[]>>>>(x77 => () => run<Pair<Kont<Kont<int>>, Kont<int[]>>>(x77, pair<Kont<Kont<int>>, Kont<int[]>>(k78 => () => run<Kont<Pair<Kont<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>, Kont<int>>>>(x79 => () => run<Pair<Kont<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>, Kont<int>>>(x79, pair<Kont<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>, Kont<int>>(k80 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(k80, w81 => () => run<Kont<int>>(w81.fst, a89 => () => run<int>(b90 => () => run<Sum<Unit, Unit>>(z84 => match<Unit, Unit>(z84, u82 => () => run<Kont<int>>(w81.fst, a85 => () => run<int>(b86 => () => run<int>(w81.snd, (a85 - b86)), 55)), u83 => () => run<Kont<int>>(w81.fst, a87 => () => run<int>(b88 => () => run<int>(w81.snd, (a87 - b88)), 48))), less(a89, b90)), 60))), k78)), w91 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(w91.fst, x108 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x108, pair<Kont<Kont<int>>, Kont<int>>(k109 => () => run<Kont<int[]>>(code, m110 => () => run<int>(i111 => () => run<int>(k109, m110[i111]), 3)), a106 => () => run<int>(b107 => () => run<int>(a98 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(w91.fst, x102 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x102, pair<Kont<Kont<int>>, Kont<int>>(k103 => () => run<Kont<int[]>>(code, m104 => () => run<int>(i105 => () => run<int>(k103, m104[i105]), 4)), a100 => () => run<int>(b101 => () => run<int>(b99 => () => run<int>(a92 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(w91.fst, x94 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x94, pair<Kont<Kont<int>>, Kont<int>>(k95 => () => run<Kont<int[]>>(code, m96 => () => run<int>(i97 => () => run<int>(k95, m96[i97]), 5)), b93 => () => run<int>(w91.snd, (a92 + b93))))), (a98 + b99)), (a100 * b101)), 16)))), (a106 * b107)), 256))))), w70.snd)), w112 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int[]>>>>(x113 => () => run<Pair<Kont<Kont<int>>, Kont<int[]>>>(x113, pair<Kont<Kont<int>>, Kont<int[]>>(k114 => () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int>>>>(x115 => () => run<Pair<Kont<Kont<int[]>>, Kont<int>>>(x115, pair<Kont<Kont<int[]>>, Kont<int>>(k116 => () => run<int>(sz165 => () => run<int[]>(m162 => () => run<int>(i163 => () => run<int>(v164 => () => run<int[]>(m159 => () => run<int>(i160 => () => run<int>(v161 => () => run<int[]>(m156 => () => run<int>(i157 => () => run<int>(v158 => () => run<int[]>(m153 => () => run<int>(i154 => () => run<int>(v155 => () => run<int[]>(m150 => () => run<int>(i151 => () => run<int>(v152 => () => run<int[]>(m147 => () => run<int>(i148 => () => run<int>(v149 => () => run<int[]>(m144 => () => run<int>(i145 => () => run<int>(v146 => () => run<int[]>(m141 => () => run<int>(i142 => () => run<int>(v143 => () => run<int[]>(m138 => () => run<int>(i139 => () => run<int>(v140 => () => run<int[]>(m135 => () => run<int>(i136 => () => run<int>(v137 => () => run<int[]>(m132 => () => run<int>(i133 => () => run<int>(v134 => () => run<int[]>(m129 => () => run<int>(i130 => () => run<int>(v131 => () => run<int[]>(m126 => () => run<int>(i127 => () => run<int>(v128 => () => run<int[]>(m123 => () => run<int>(i124 => () => run<int>(v125 => () => run<int[]>(m120 => () => run<int>(i121 => () => run<int>(v122 => () => run<int[]>(m117 => () => run<int>(i118 => () => run<int>(v119 => () => run<int[]>(k116, set(m117, i118, v119)), 69), 15), set(m120, i121, v122)), 239), 14), set(m123, i124, v125)), 115), 13), set(m126, i127, v128)), 209), 12), set(m129, i130, v131)), 241), 11), set(m132, i133, v134)), 71), 10), set(m135, i136, v137)), 22), 9), set(m138, i139, v140)), 158), 8), set(m141, i142, v143)), 215), 7), set(m144, i145, v146)), 66), 6), set(m147, i148, v149)), 106), 5), set(m150, i151, v152)), 156), 4), set(m153, i154, v155)), 135), 3), set(m156, i157, v158)), 156), 2), set(m159, i160, v161)), 218), 1), set(m162, i163, v164)), 103), 0), new int[sz165]), 16), k114)), w166 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(x167 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x167, pair<Kont<Kont<int>>, Kont<int>>(k168 => () => run<int>(k168, 0), w166.snd)), unrec<Pair<Kont<Kont<int>>, Kont<int>>>((w169, rec170) => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(x172 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x172, pair<Kont<Kont<int>>, Kont<int>>(k173 => () => run<Kont<int[]>>(w166.fst, m182 => () => run<Kont<int>>(w169.fst, i183 => () => run<int>(a176 => () => run<Kont<int[]>>(email, m178 => () => run<Kont<int>>(w169.fst, a180 => () => run<Kont<int>>(elen, b181 => () => run<int>(i179 => () => run<int>(b177 => () => run<int>(a174 => () => run<int>(b175 => () => run<int>(k173, (a174 % b175)), 256), (a176 * b177)), m178[i179]), (a180 % b181))))), m182[i183]))), w169.snd)), w184 => () => run<Kont<int>>(w169.fst, a194 => () => run<int>(b195 => () => run<Sum<Unit, Unit>>(z187 => match<Unit, Unit>(z187, u185 => () => run<Kont<int>>(w184.fst, a188 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(u171 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(u171, rec170), x190 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x190, pair<Kont<Kont<int>>, Kont<int>>(k191 => () => run<Kont<int>>(w169.fst, a192 => () => run<int>(b193 => () => run<int>(k191, (a192 + b193)), 1)), b189 => () => run<int>(w184.snd, (a188 + b189)))))), u186 => () => run<Kont<int>>(w184.fst, w184.snd)), eql(a194, b195)), 15)))))), w112.snd)), w196 => () => run<Kont<int>>(w112.fst, a213 => () => run<Kont<int>>(w196.fst, b214 => () => run<Sum<Unit, Unit>>(z199 => match<Unit, Unit>(z199, u197 => () => run<Kont<int[]>>(w70.fst, w196.snd), u198 => () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int[]>>>>(x200 => () => run<Pair<Kont<Kont<int[]>>, Kont<int[]>>>(x200, pair<Kont<Kont<int[]>>, Kont<int[]>>(k201 => () => run<Kont<int[]>>(w70.fst, m202 => () => run<int>(i203 => () => run<int>(v204 => () => run<int[]>(k201, set(m202, i203, v204)), 1), 1)), w196.snd)), w205 => () => run<Kont<int[]>>(w205.fst, m206 => () => run<int>(i207 => () => run<Kont<int[]>>(w205.fst, m211 => () => run<int>(i212 => () => run<int>(a209 => () => run<int>(b210 => () => run<int>(v208 => () => run<int[]>(w205.snd, set(m206, i207, v208)), (a209 + b210)), 1), m211[i212]), 27)), 27)))), eql(a213, b214)))))), u75 => () => run<Kont<int[]>>(w70.fst, w70.snd)), less(a215, b216)), 6)), u72 => () => run<Kont<int[]>>(w70.fst, w70.snd)), less(a217, b218)), 1)));
            try
            {
                while (true)
                {
                    fmain = fmain();
                }
            }
            catch (Res<int[]> rr)
            {
                /*var a = rr.res;
                var s11 = a[11] + a[24] + a[66];
                var s32 = a[32] + a[07] + a[55];
                var s69 = a[69] + a[81] + a[15];
                var s27 = a[27] + a[14] + a[52];
                Console.WriteLine("result: {0} {1} {2} {3} {4} {5} {6}", a[0], a[1], s11, s32, s69, s27, a[93]);
                Console.WriteLine("R: {0} {1}", R[0], R[1]);*/
            }
            evtDone.Set();
            Program.mainform.BeginInvoke(new MethodInvoker(Program.mainform.HideRegisterButton));
            return R[1] > 0;
        }
    }//class
}
