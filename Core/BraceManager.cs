using System;
using System.Collections.Generic;
using System.Text;

namespace MUI.Components.ECMAScriptDealer
{
    /// <summary>
    /// 所有大括号单元映像管理类
    /// </summary>
    public class BraceManager
    {
        Dictionary<int, Brace> hash_brace = new Dictionary<int, Brace>();//用于存放所有的大括号单元映像

        public void Add(int index_word, Brace brace)
        {
            this.hash_brace.Add(index_word, brace);
        }
        public Brace Get(int index_word)
        {
            return this.hash_brace[index_word];
        }
        public void Clear()
        {            
            foreach (int i in this.hash_brace.Keys)
            {
                this.hash_brace[Convert.ToInt32(i)].Dispose();
            }
            this.hash_brace.Clear();
        }
    }
}
