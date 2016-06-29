using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

public class Singleton<TYPE> where TYPE : new() {
    public static TYPE Instance() {
        if (m_instance == null) {
            m_instance = new TYPE();
        }

        //Log.Assert(m_instance != null, "singleton is null");
        return m_instance;
    }

    // constructor
    #region
    protected Singleton() { }
    private   Singleton(ref Singleton<TYPE> instance) { }
    private   Singleton(Singleton<TYPE> instance) { }
    #endregion

    // member
    #region
    private static TYPE m_instance;
    #endregion
}

