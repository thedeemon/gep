using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Reflection;

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

        public void CalcDays(Type appty)
        {
            Type dty = Type.GetType("System.DateTime");
            var now = dty.GetProperty("Now");
            DateTime dt2 = (DateTime)now.GetValue(null, null); 
            var pathprop = appty.GetProperty("StartupPath");
            var ps = (string)pathprop.GetValue(null, null);
            var di = new DirectoryInfo(ps);
            var fd = di.CreationTime;
            TimeSpan ts = dt2 - fd;
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

            //Thunk fmain = () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int[]>>>>(x0 => () => run<Pair<Kont<Kont<int[]>>, Kont<int[]>>>(x0, pair<Kont<Kont<int[]>>, Kont<int[]>>(k1 => () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int[]>>>>(x2 => () => run<Pair<Kont<Kont<int[]>>, Kont<int[]>>>(x2, pair<Kont<Kont<int[]>>, Kont<int[]>>(k3 => () => run<Kont<int[]>>(r, m44 => () => run<int>(i45 => () => run<int>(a47 => () => run<Kont<int[]>>(r, m53 => () => run<int>(i54 => () => run<int>(a49 => () => run<Kont<int[]>>(r, m51 => () => run<int>(i52 => () => run<int>(b50 => () => run<int>(b48 => () => run<int>(v46 => () => run<int[]>(m31 => () => run<int>(i32 => () => run<int>(a34 => () => run<Kont<int[]>>(r, m42 => () => run<int>(i43 => () => run<int>(a38 => () => run<Kont<int[]>>(r, m40 => () => run<int>(i41 => () => run<int>(b39 => () => run<int>(a36 => () => run<int>(b37 => () => run<int>(b35 => () => run<int>(v33 => () => run<int[]>(m18 => () => run<int>(i19 => () => run<int>(a21 => () => run<Kont<int[]>>(r, m29 => () => run<int>(i30 => () => run<int>(a25 => () => run<Kont<int[]>>(r, m27 => () => run<int>(i28 => () => run<int>(b26 => () => run<int>(a23 => () => run<int>(b24 => () => run<int>(b22 => () => run<int>(v20 => () => run<int[]>(m7 => () => run<int>(i8 => () => run<int>(a10 => () => run<Kont<int[]>>(r, m16 => () => run<int>(i17 => () => run<int>(a12 => () => run<Kont<int[]>>(r, m14 => () => run<int>(i15 => () => run<int>(b13 => () => run<int>(b11 => () => run<int>(v9 => () => run<int[]>(m4 => () => run<int>(i5 => () => run<int>(v6 => () => run<int[]>(k3, set(m4, i5, v6)), 1), 0), set(m7, i8, v9)), (a10 - b11)), (a12 + b13)), m14[i15]), 52)), m16[i17]), 14)), 0), 27), set(m18, i19, v20)), (a21 - b22)), (a23 - b24)), 175), (a25 + b26)), m27[i28]), 15)), m29[i30]), 81)), 0), 69), set(m31, i32, v33)), (a34 - b35)), (a36 - b37)), 1), (a38 + b39)), m40[i41]), 55)), m42[i43]), 7)), 0), 32), set(m44, i45, v46)), (a47 - b48)), (a49 + b50)), m51[i52]), 66)), m53[i54]), 24)), 0), 11)), k1)), w55 => () => run<Kont<int[]>>(w55.fst, m68 => () => run<int>(i69 => () => run<int>(a66 => () => run<int>(b67 => () => run<Sum<Unit, Unit>>(z58 => match<Unit, Unit>(z58, u56 => () => run<Kont<int[]>>(w55.fst, w55.snd), u57 => () => run<Kont<int[]>>(w55.fst, m59 => () => run<int>(i60 => () => run<int>(a62 => () => run<Kont<int[]>>(w55.fst, m64 => () => run<int>(i65 => () => run<int>(b63 => () => run<int>(v61 => () => run<int[]>(w55.snd, set(m59, i60, v61)), (a62 - b63)), m64[i65]), 13)), 30), 93))), less(a66, b67)), 31), m68[i69]), 13))), x => { throw new Res<int[]>(x); })), w70 => () => run<Kont<int>>(elen, a217 => () => run<int>(b218 => () => run<Sum<Unit, Unit>>(z73 => match<Unit, Unit>(z73, u71 => () => run<Kont<int>>(codelen, a215 => () => run<int>(b216 => () => run<Sum<Unit, Unit>>(z76 => match<Unit, Unit>(z76, u74 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int[]>>>>(x77 => () => run<Pair<Kont<Kont<int>>, Kont<int[]>>>(x77, pair<Kont<Kont<int>>, Kont<int[]>>(k78 => () => run<Kont<Pair<Kont<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>, Kont<int>>>>(x79 => () => run<Pair<Kont<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>, Kont<int>>>(x79, pair<Kont<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>, Kont<int>>(k80 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(k80, w81 => () => run<Kont<int>>(w81.fst, a89 => () => run<int>(b90 => () => run<Sum<Unit, Unit>>(z84 => match<Unit, Unit>(z84, u82 => () => run<Kont<int>>(w81.fst, a85 => () => run<int>(b86 => () => run<int>(w81.snd, (a85 - b86)), 55)), u83 => () => run<Kont<int>>(w81.fst, a87 => () => run<int>(b88 => () => run<int>(w81.snd, (a87 - b88)), 48))), less(a89, b90)), 60))), k78)), w91 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(w91.fst, x108 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x108, pair<Kont<Kont<int>>, Kont<int>>(k109 => () => run<Kont<int[]>>(code, m110 => () => run<int>(i111 => () => run<int>(k109, m110[i111]), 3)), a106 => () => run<int>(b107 => () => run<int>(a98 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(w91.fst, x102 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x102, pair<Kont<Kont<int>>, Kont<int>>(k103 => () => run<Kont<int[]>>(code, m104 => () => run<int>(i105 => () => run<int>(k103, m104[i105]), 4)), a100 => () => run<int>(b101 => () => run<int>(b99 => () => run<int>(a92 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(w91.fst, x94 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x94, pair<Kont<Kont<int>>, Kont<int>>(k95 => () => run<Kont<int[]>>(code, m96 => () => run<int>(i97 => () => run<int>(k95, m96[i97]), 5)), b93 => () => run<int>(w91.snd, (a92 + b93))))), (a98 + b99)), (a100 * b101)), 16)))), (a106 * b107)), 256))))), w70.snd)), w112 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int[]>>>>(x113 => () => run<Pair<Kont<Kont<int>>, Kont<int[]>>>(x113, pair<Kont<Kont<int>>, Kont<int[]>>(k114 => () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int>>>>(x115 => () => run<Pair<Kont<Kont<int[]>>, Kont<int>>>(x115, pair<Kont<Kont<int[]>>, Kont<int>>(k116 => () => run<int>(sz165 => () => run<int[]>(m162 => () => run<int>(i163 => () => run<int>(v164 => () => run<int[]>(m159 => () => run<int>(i160 => () => run<int>(v161 => () => run<int[]>(m156 => () => run<int>(i157 => () => run<int>(v158 => () => run<int[]>(m153 => () => run<int>(i154 => () => run<int>(v155 => () => run<int[]>(m150 => () => run<int>(i151 => () => run<int>(v152 => () => run<int[]>(m147 => () => run<int>(i148 => () => run<int>(v149 => () => run<int[]>(m144 => () => run<int>(i145 => () => run<int>(v146 => () => run<int[]>(m141 => () => run<int>(i142 => () => run<int>(v143 => () => run<int[]>(m138 => () => run<int>(i139 => () => run<int>(v140 => () => run<int[]>(m135 => () => run<int>(i136 => () => run<int>(v137 => () => run<int[]>(m132 => () => run<int>(i133 => () => run<int>(v134 => () => run<int[]>(m129 => () => run<int>(i130 => () => run<int>(v131 => () => run<int[]>(m126 => () => run<int>(i127 => () => run<int>(v128 => () => run<int[]>(m123 => () => run<int>(i124 => () => run<int>(v125 => () => run<int[]>(m120 => () => run<int>(i121 => () => run<int>(v122 => () => run<int[]>(m117 => () => run<int>(i118 => () => run<int>(v119 => () => run<int[]>(k116, set(m117, i118, v119)), 69), 15), set(m120, i121, v122)), 239), 14), set(m123, i124, v125)), 115), 13), set(m126, i127, v128)), 209), 12), set(m129, i130, v131)), 241), 11), set(m132, i133, v134)), 71), 10), set(m135, i136, v137)), 22), 9), set(m138, i139, v140)), 158), 8), set(m141, i142, v143)), 215), 7), set(m144, i145, v146)), 66), 6), set(m147, i148, v149)), 106), 5), set(m150, i151, v152)), 156), 4), set(m153, i154, v155)), 135), 3), set(m156, i157, v158)), 156), 2), set(m159, i160, v161)), 218), 1), set(m162, i163, v164)), 103), 0), new int[sz165]), 16), k114)), w166 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(x167 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x167, pair<Kont<Kont<int>>, Kont<int>>(k168 => () => run<int>(k168, 0), w166.snd)), unrec<Pair<Kont<Kont<int>>, Kont<int>>>((w169, rec170) => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(x172 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x172, pair<Kont<Kont<int>>, Kont<int>>(k173 => () => run<Kont<int[]>>(w166.fst, m182 => () => run<Kont<int>>(w169.fst, i183 => () => run<int>(a176 => () => run<Kont<int[]>>(email, m178 => () => run<Kont<int>>(w169.fst, a180 => () => run<Kont<int>>(elen, b181 => () => run<int>(i179 => () => run<int>(b177 => () => run<int>(a174 => () => run<int>(b175 => () => run<int>(k173, (a174 % b175)), 256), (a176 * b177)), m178[i179]), (a180 % b181))))), m182[i183]))), w169.snd)), w184 => () => run<Kont<int>>(w169.fst, a194 => () => run<int>(b195 => () => run<Sum<Unit, Unit>>(z187 => match<Unit, Unit>(z187, u185 => () => run<Kont<int>>(w184.fst, a188 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(u171 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(u171, rec170), x190 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x190, pair<Kont<Kont<int>>, Kont<int>>(k191 => () => run<Kont<int>>(w169.fst, a192 => () => run<int>(b193 => () => run<int>(k191, (a192 + b193)), 1)), b189 => () => run<int>(w184.snd, (a188 + b189)))))), u186 => () => run<Kont<int>>(w184.fst, w184.snd)), eql(a194, b195)), 15)))))), w112.snd)), w196 => () => run<Kont<int>>(w112.fst, a213 => () => run<Kont<int>>(w196.fst, b214 => () => run<Sum<Unit, Unit>>(z199 => match<Unit, Unit>(z199, u197 => () => run<Kont<int[]>>(w70.fst, w196.snd), u198 => () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int[]>>>>(x200 => () => run<Pair<Kont<Kont<int[]>>, Kont<int[]>>>(x200, pair<Kont<Kont<int[]>>, Kont<int[]>>(k201 => () => run<Kont<int[]>>(w70.fst, m202 => () => run<int>(i203 => () => run<int>(v204 => () => run<int[]>(k201, set(m202, i203, v204)), 1), 1)), w196.snd)), w205 => () => run<Kont<int[]>>(w205.fst, m206 => () => run<int>(i207 => () => run<Kont<int[]>>(w205.fst, m211 => () => run<int>(i212 => () => run<int>(a209 => () => run<int>(b210 => () => run<int>(v208 => () => run<int[]>(w205.snd, set(m206, i207, v208)), (a209 + b210)), 1), m211[i212]), 27)), 27)))), eql(a213, b214)))))), u75 => () => run<Kont<int[]>>(w70.fst, w70.snd)), less(a215, b216)), 6)), u72 => () => run<Kont<int[]>>(w70.fst, w70.snd)), less(a217, b218)), 1)));
            Thunk fmain = () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int[]>>>>(x0 => () => run<Pair<Kont<Kont<int[]>>, Kont<int[]>>>(x0, pair<Kont<Kont<int[]>>, Kont<int[]>>(k1 => () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int[]>>>>(x2 => () => run<Pair<Kont<Kont<int[]>>, Kont<int[]>>>(x2, pair<Kont<Kont<int[]>>, Kont<int[]>>(k3 => () => run<Kont<int[]>>(r, m41 => () => run<int>(i42 => () => run<int>(a44 => () => run<Kont<int[]>>(r, m50 => () => run<int>(i51 => () => run<int>(a46 => () => run<Kont<int[]>>(r, m48 => () => run<int>(i49 => () => run<int>(b47 => () => run<int>(b45 => () => run<int>(v43 => () => run<int[]>(m28 => () => run<int>(i29 => () => run<int>(a31 => () => run<Kont<int[]>>(r, m39 => () => run<int>(i40 => () => run<int>(a35 => () => run<Kont<int[]>>(r, m37 => () => run<int>(i38 => () => run<int>(b36 => () => run<int>(a33 => () => run<int>(b34 => () => run<int>(b32 => () => run<int>(v30 => () => run<int[]>(m15 => () => run<int>(i16 => () => run<int>(a18 => () => run<Kont<int[]>>(r, m26 => () => run<int>(i27 => () => run<int>(a22 => () => run<Kont<int[]>>(r, m24 => () => run<int>(i25 => () => run<int>(b23 => () => run<int>(a20 => () => run<int>(b21 => () => run<int>(b19 => () => run<int>(v17 => () => run<int[]>(m4 => () => run<int>(i5 => () => run<int>(a7 => () => run<Kont<int[]>>(r, m13 => () => run<int>(i14 => () => run<int>(a9 => () => run<Kont<int[]>>(r, m11 => () => run<int>(i12 => () => run<int>(b10 => () => run<int>(b8 => () => run<int>(v6 => () => run<int[]>(k3, set(m4, i5, v6)), (a7 - b8)), (a9 + b10)), m11[i12]), 52)), m13[i14]), 14)), 0), 27), set(m15, i16, v17)), (a18 - b19)), (a20 - b21)), 175), (a22 + b23)), m24[i25]), 15)), m26[i27]), 81)), 0), 69), set(m28, i29, v30)), (a31 - b32)), (a33 - b34)), 1), (a35 + b36)), m37[i38]), 55)), m39[i40]), 7)), 0), 32), set(m41, i42, v43)), (a44 - b45)), (a46 + b47)), m48[i49]), 66)), m50[i51]), 24)), 0), 11)), k1)), w52 => () => run<Kont<int[]>>(w52.fst, m65 => () => run<int>(i66 => () => run<int>(a63 => () => run<int>(b64 => () => run<Sum<Unit, Unit>>(z55 => match<Unit, Unit>(z55, u53 => () => run<Kont<int[]>>(w52.fst, w52.snd), u54 => () => run<Kont<int[]>>(w52.fst, m56 => () => run<int>(i57 => () => run<int>(a59 => () => run<Kont<int[]>>(w52.fst, m61 => () => run<int>(i62 => () => run<int>(b60 => () => run<int>(v58 => () => run<int[]>(w52.snd, set(m56, i57, v58)), (a59 - b60)), m61[i62]), 13)), 30), 93))), less(a63, b64)), 31), m65[i66]), 13))), x => { throw new Res<int[]>(x); })), w67 => () => run<Kont<int>>(elen, a226 => () => run<int>(b227 => () => run<Sum<Unit, Unit>>(z70 => match<Unit, Unit>(z70, u68 => () => run<Kont<int>>(codelen, a221 => () => run<int>(b222 => () => run<Sum<Unit, Unit>>(z73 => match<Unit, Unit>(z73, u71 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int[]>>>>(x74 => () => run<Pair<Kont<Kont<int>>, Kont<int[]>>>(x74, pair<Kont<Kont<int>>, Kont<int[]>>(k75 => () => run<Kont<Pair<Kont<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>, Kont<int>>>>(x76 => () => run<Pair<Kont<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>, Kont<int>>>(x76, pair<Kont<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>, Kont<int>>(k77 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(k77, w78 => () => run<Kont<int>>(w78.fst, a86 => () => run<int>(b87 => () => run<Sum<Unit, Unit>>(z81 => match<Unit, Unit>(z81, u79 => () => run<Kont<int>>(w78.fst, a82 => () => run<int>(b83 => () => run<int>(w78.snd, (a82 - b83)), 55)), u80 => () => run<Kont<int>>(w78.fst, a84 => () => run<int>(b85 => () => run<int>(w78.snd, (a84 - b85)), 48))), less(a86, b87)), 60))), k75)), w88 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(w88.fst, x105 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x105, pair<Kont<Kont<int>>, Kont<int>>(k106 => () => run<Kont<int[]>>(code, m107 => () => run<int>(i108 => () => run<int>(k106, m107[i108]), 3)), a103 => () => run<int>(b104 => () => run<int>(a95 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(w88.fst, x99 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x99, pair<Kont<Kont<int>>, Kont<int>>(k100 => () => run<Kont<int[]>>(code, m101 => () => run<int>(i102 => () => run<int>(k100, m101[i102]), 4)), a97 => () => run<int>(b98 => () => run<int>(b96 => () => run<int>(a89 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(w88.fst, x91 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x91, pair<Kont<Kont<int>>, Kont<int>>(k92 => () => run<Kont<int[]>>(code, m93 => () => run<int>(i94 => () => run<int>(k92, m93[i94]), 5)), b90 => () => run<int>(w88.snd, (a89 + b90))))), (a95 + b96)), (a97 * b98)), 16)))), (a103 * b104)), 256))))), w67.snd)), w109 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int[]>>>>(x110 => () => run<Pair<Kont<Kont<int>>, Kont<int[]>>>(x110, pair<Kont<Kont<int>>, Kont<int[]>>(k111 => () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int>>>>(x112 => () => run<Pair<Kont<Kont<int[]>>, Kont<int>>>(x112, pair<Kont<Kont<int[]>>, Kont<int>>(k113 => () => run<int>(sz162 => () => run<int[]>(m159 => () => run<int>(i160 => () => run<int>(v161 => () => run<int[]>(m156 => () => run<int>(i157 => () => run<int>(v158 => () => run<int[]>(m153 => () => run<int>(i154 => () => run<int>(v155 => () => run<int[]>(m150 => () => run<int>(i151 => () => run<int>(v152 => () => run<int[]>(m147 => () => run<int>(i148 => () => run<int>(v149 => () => run<int[]>(m144 => () => run<int>(i145 => () => run<int>(v146 => () => run<int[]>(m141 => () => run<int>(i142 => () => run<int>(v143 => () => run<int[]>(m138 => () => run<int>(i139 => () => run<int>(v140 => () => run<int[]>(m135 => () => run<int>(i136 => () => run<int>(v137 => () => run<int[]>(m132 => () => run<int>(i133 => () => run<int>(v134 => () => run<int[]>(m129 => () => run<int>(i130 => () => run<int>(v131 => () => run<int[]>(m126 => () => run<int>(i127 => () => run<int>(v128 => () => run<int[]>(m123 => () => run<int>(i124 => () => run<int>(v125 => () => run<int[]>(m120 => () => run<int>(i121 => () => run<int>(v122 => () => run<int[]>(m117 => () => run<int>(i118 => () => run<int>(v119 => () => run<int[]>(m114 => () => run<int>(i115 => () => run<int>(v116 => () => run<int[]>(k113, set(m114, i115, v116)), 69), 15), set(m117, i118, v119)), 239), 14), set(m120, i121, v122)), 115), 13), set(m123, i124, v125)), 209), 12), set(m126, i127, v128)), 241), 11), set(m129, i130, v131)), 71), 10), set(m132, i133, v134)), 22), 9), set(m135, i136, v137)), 158), 8), set(m138, i139, v140)), 215), 7), set(m141, i142, v143)), 66), 6), set(m144, i145, v146)), 106), 5), set(m147, i148, v149)), 156), 4), set(m150, i151, v152)), 135), 3), set(m153, i154, v155)), 156), 2), set(m156, i157, v158)), 218), 1), set(m159, i160, v161)), 103), 0), new int[sz162]), 16), k111)), w163 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(x164 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x164, pair<Kont<Kont<int>>, Kont<int>>(k165 => () => run<int>(k165, 0), w163.snd)), unrec<Pair<Kont<Kont<int>>, Kont<int>>>((w166, rec167) => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(x169 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x169, pair<Kont<Kont<int>>, Kont<int>>(k170 => () => run<Kont<int[]>>(w163.fst, m179 => () => run<Kont<int>>(w166.fst, i180 => () => run<int>(a173 => () => run<Kont<int[]>>(email, m175 => () => run<Kont<int>>(w166.fst, a177 => () => run<Kont<int>>(elen, b178 => () => run<int>(i176 => () => run<int>(b174 => () => run<int>(a171 => () => run<int>(b172 => () => run<int>(k170, (a171 % b172)), 256), (a173 * b174)), m175[i176]), (a177 % b178))))), m179[i180]))), w166.snd)), w181 => () => run<Kont<int>>(w166.fst, a191 => () => run<int>(b192 => () => run<Sum<Unit, Unit>>(z184 => match<Unit, Unit>(z184, u182 => () => run<Kont<int>>(w181.fst, a185 => () => run<Kont<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>>(u168 => () => run<Kont<Pair<Kont<Kont<int>>, Kont<int>>>>(u168, rec167), x187 => () => run<Pair<Kont<Kont<int>>, Kont<int>>>(x187, pair<Kont<Kont<int>>, Kont<int>>(k188 => () => run<Kont<int>>(w166.fst, a189 => () => run<int>(b190 => () => run<int>(k188, (a189 + b190)), 1)), b186 => () => run<int>(w181.snd, (a185 + b186)))))), u183 => () => run<Kont<int>>(w181.fst, w181.snd)), eql(a191, b192)), 15)))))), w109.snd)), w193 => () => run<Kont<int>>(w109.fst, a216 => () => run<Kont<int>>(w193.fst, b217 => () => run<Sum<Unit, Unit>>(z196 => match<Unit, Unit>(z196, u194 => () => run<Kont<int[]>>(w67.fst, m197 => () => run<int>(i198 => () => run<int>(v199 => () => run<int[]>(w193.snd, set(m197, i198, v199)), 1), 0)), u195 => () => run<Kont<Pair<Kont<Kont<int[]>>, Kont<int[]>>>>(x203 => () => run<Pair<Kont<Kont<int[]>>, Kont<int[]>>>(x203, pair<Kont<Kont<int[]>>, Kont<int[]>>(k204 => () => run<Kont<int[]>>(w67.fst, m205 => () => run<int>(i206 => () => run<int>(v207 => () => run<int[]>(k204, set(m205, i206, v207)), 1), 1)), m200 => () => run<int>(i201 => () => run<int>(v202 => () => run<int[]>(w193.snd, set(m200, i201, v202)), 1), 0))), w208 => () => run<Kont<int[]>>(w208.fst, m209 => () => run<int>(i210 => () => run<Kont<int[]>>(w208.fst, m214 => () => run<int>(i215 => () => run<int>(a212 => () => run<int>(b213 => () => run<int>(v211 => () => run<int[]>(w208.snd, set(m209, i210, v211)), (a212 + b213)), 1), m214[i215]), 27)), 27)))), eql(a216, b217)))))), u72 => () => run<Kont<int[]>>(w67.fst, m218 => () => run<int>(i219 => () => run<int>(v220 => () => run<int[]>(w67.snd, set(m218, i219, v220)), 1), 0))), less(a221, b222)), 6)), u69 => () => run<Kont<int[]>>(w67.fst, m223 => () => run<int>(i224 => () => run<int>(v225 => () => run<int[]>(w67.snd, set(m223, i224, v225)), 1), 0))), less(a226, b227)), 1))); 

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
