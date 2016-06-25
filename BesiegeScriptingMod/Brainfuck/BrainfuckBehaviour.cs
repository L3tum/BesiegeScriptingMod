using System;
using System.Collections;
using UnityEngine;

namespace BesiegeScriptingMod.Brainfuck
{
    class BrainfuckBehaviour : MonoBehaviour
    {
        private readonly int _popupId = spaar.ModLoader.Util.GetWindowID();
        Rect _winRect = new Rect(Screen.width / 2.0f - 50.0f, Screen.height / 2.0f - 50.0f, 100.0f, 100.0f);
        // Instance info.
        // Instance of Brainfuck interpreter, actually handles interpreting.

        /** The array that acts as the interpreter's virtual memory */
        private char[] mem;
        /** The memory pointer */
        private int mp;

        /** The string containing the commands to be executed */
        private char[] com;

        /** The instruction pointer */
        private int ip = 0;

        private int EOF; //End Of File

        public String sauce = "";
        private System.Random _rand;
        private bool _awaiting;

        public void init(string code)
        {
                mem = new char[30000];
                mp = 0;
                com = code.ToCharArray();
                EOF = com.Length;
                StartCoroutine_Auto(runBF());
            _rand = new System.Random(DateTime.Now.Millisecond + DateTime.Now.Second + DateTime.Now.Minute + DateTime.Now.Year + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Month);
        }
        public void OnGUI()
        {
            if (_awaiting)
            {
                _winRect = GUI.Window(_popupId, _winRect, Func, "Popup");
            }
        }
        private void Func(int id)
        {
            GUILayout.Label("Awaiting input...");
            if (Input.anyKeyDown && Input.inputString.Length > 0)
            {
                GUILayout.Label(Input.inputString[0].ToString());
            }
            GUI.DragWindow();
        }

        public IEnumerator GetAsciiOfKeyDown(int mps, int key)
        {
            _awaiting = true;
            do
            {
                yield return null;
            } while (!Input.anyKeyDown || Input.inputString.Length < 1);
            Debug.Log(Input.inputString[0]);
            mem[mps] = Input.inputString[0];
            _awaiting = false;
        }

        public void Done()
        {
            GetComponent<ScriptHandler>()._cSauce = sauce;
            GetComponent<ScriptHandler>()._displayC = true;
            Destroy(this);
        }

        /**
            * Run the interpreter with its given string
            */

        public IEnumerator runBF()
        {
            while (ip < EOF)
            {
                // Get the current command
                char c = com[ip];

                // Act based on the current command and the brainfuck spec
                switch (c)
                {
                    case '>':
                        mp++;
                        break;
                    case '<':
                        mp--;
                        break;
                    case '+':
                        mem[mp]++;
                        break;
                    case '-':
                        mem[mp]--;
                        break;
                    case '.':
                        sauce += (mem[mp]);
                        break;
                    case ',':
                        int key = _rand.Next();
                        yield return StartCoroutine_Auto(GetAsciiOfKeyDown(mp, key));
                        break;
                    case '[':
                        if (mem[mp] == 0)
                        {
                            while (com[ip] != ']') ip++;
                        }
                        break;

                    case ']':
                        if (mem[mp] != 0)
                        {
                            while (com[ip] != '[') ip--;
                        }
                        break;
                }

                // increment instruction mp
                ip++;
            }
            yield return null;
            Done();
        }
    }
}
