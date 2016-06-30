using System;
using System.Collections;
using UnityEngine;
using Random = System.Random;

namespace BesiegeScriptingMod.Ook
{
    class OokBehaviour : MonoBehaviour
    {
        private readonly int _popupId = spaar.ModLoader.Util.GetWindowID();
        Rect _winRect = new Rect(Screen.width/2.0f -50.0f, Screen.height/2.0f - 50.0f, 100.0f, 100.0f);
        // Instance info.
        // Instance of Brainfuck interpreter, actually handles interpreting.

        /** The array that acts as the interpreter's virtual memory */
        private char[] mem;
        /** The memory pointer */
        private int mp;

        /** The Strings containing the commands to be executed */
        private String[] ookCom;

        /** The instruction pointer */
        private int ip = 0;

        private int EOF; //End Of File

        public String sauce = "";
        private Random _rand;
        private bool _awaiting;

        /**
            * Create the Brainfuck VM and give it the string to be interpreted.
            * @param s The string to be interpreted.
            */

        public void init(string code)
        {
                mem = new char[30000];
                mp = 0;
                ookCom = Util.Util.splitStringAtNewlineAndSpace(code);
                EOF = ookCom.Length;
                StartCoroutine_Auto(runOok());
            _rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Second + DateTime.Now.Minute + DateTime.Now.Year + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Month);
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

        public IEnumerator runOok()
        {
            while (ip + 1 < EOF)
            {
                // Get the current command
                String c = ookCom[ip] + ookCom[ip + 1];

                // Act based on the current command and the brainfuck spec
                switch (c)
                {
                    case "Ook.Ook?":
                        mp++;
                        break;
                    case "Ook?Ook.":
                        mp--;
                        break;
                    case "Ook.Ook.":
                        mem[mp]++;
                        break;
                    case "Ook!Ook!":
                        mem[mp]--;
                        break;
                    case "Ook!Ook.":
                        sauce += (mem[mp]);
                        break;
                    case "Ook.Ook!":
                        int key = _rand.Next();
                        yield return StartCoroutine_Auto(GetAsciiOfKeyDown(mp, key));
                        break;
                    case "Ook!Ook?":
                        if (mem[mp] == 0)
                        {
                            while (ookCom[ip] + ookCom[ip+1] != "Ook?Ook!")
                            { 
                                ip++;
                            }
                        }
                        break;

                    case "Ook?Ook!":
                        if (mem[mp] != 0)
                        {
                            while (ookCom[ip] + ookCom[ip+1] != "Ook!Ook?")
                            {
                                ip--;
                            }
                        }
                        break;
                }

                // increment instruction mp
                ip += 2;
            }
            Done();
            yield return null;
        }
    }
}