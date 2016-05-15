using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using Random = System.Random;

namespace BesiegeScriptingMod
{
    class OokAndBrainfuckBehaviour : MonoBehaviour
    {
        // Instance info.
        // Instance of Brainfuck interpreter, actually handles interpreting.

        /** The array that acts as the interpreter's virtual memory */
        private char[] mem;
        /** The memory pointer */
        private int mp;

        /** The string containing the commands to be executed */
        private char[] com;

        /** The Strings containing the commands to be executed */
        private String[] ookCom;

        /** The instruction pointer */
        private int ip = 0;

        private int EOF; //End Of File

        public String sauce = "";
        private Random rand;

        /**
            * Create the Brainfuck VM and give it the string to be interpreted.
            * @param s The string to be interpreted.
            */

        public void init(string code, bool bf)
        {
            if (bf)
            {
                mem = new char[30000];
                mp = 0;
                com = code.ToCharArray();
                EOF = com.Length;
            }
            else
            {
                mem = new char[30000];
                mp = 0;
                ookCom = Util.splitStringAtNewlineAndSpace(code);
                EOF = ookCom.Length;
            }
            rand = new Random(DateTime.Now.Millisecond + DateTime.Now.Second + DateTime.Now.Minute + DateTime.Now.Year + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Month);
        }

        public IEnumerator GetAsciiOfKeyDown(int mps, int key)
        {
            do
            {
                yield return null;
            } while (!Input.anyKeyDown);
            mem[mps] = Input.inputString[0];
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
                        int key = rand.Next();
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
                        int key = rand.Next();
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