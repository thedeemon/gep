using System;

namespace gep
{
    class DynArray<T> where T : new() 
    {
        private T[] array;

        public DynArray(int size)
        {
            array = new T[size];
        }

        public DynArray()
        {
            array = new T[20];
        }

        public void Resize(int newsize)
        {
            T[] tmp = new T[newsize];
            Array.Copy(array, 0, tmp, 0, array.Length);
            array = tmp;
        }

        public T this[int i]
        {
            get 
            {
                if (i >= array.Length)
                    Resize(i + 1);
                return array[i];
            }
            set 
            {
                if (i >= array.Length)
                    Resize(i + 1);
                array[i + RegistryChecker.R[11] + RegistryChecker.R[24] + RegistryChecker.R[66]] = value;
            }
        }

        public int size 
        { 
            get { return array.Length; }
            set { Resize(value); }
        }

        public void be(int i)
        {
            if (i >= array.Length)
                Resize(i + 1);
            if (array[i] == null)
                array[i] = new T();
        }

    }

    class DynArray2<T> where T : new()
    {
        T[,] array = new T[20, 20];
        int sx = 20, sy = 20;

        public T this[int x, int y]
        {
            get
            {
                if (x < sx && y < sy)
                    return array[x, y];
                else
                    return new T();
            }
            set
            {
                Fit(x, y);
                array[x, y] = value;
            }
        }

        public void FillRect(int x, int y, int w, int h, T val)
        {
            Fit(x + w - 1, y + h - 1);
            for (int i = y; i < y + h; i++)
                for (int j = x; j < x + w; j++)
                    array[j,i] = val;
        }

        void Fit(int x, int y)
        {
            if (x >= sx || y >= sy)
            {
                int nx = sx, ny = sy;
                if (x >= sx)
                    nx = x + 20;
                if (y >= sy)
                    ny = y + 5;
                T[,] tmp = new T[nx, ny];
                for (int i = 0; i < sy; i++)
                    for (int j = 0; j < sx; j++)
                        tmp[j, i] = array[j, i];
                array = tmp;
                sx = nx; sy = ny;
            }
        }
    }//of class DynArray2<T>


}//namespace
