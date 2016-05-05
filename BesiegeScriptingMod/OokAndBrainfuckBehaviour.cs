using System;
using System.Collections.Generic;
using UnityEngine;

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

        public char result = ' ';

        /**
            * Create the Brainfuck VM and give it the string to be interpreted.
            * @param s The string to be interpreted.
            */

        public OokAndBrainfuckBehaviour(string code, bool bf)
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
        }

        public IEnumerator<char> GetAsciiOfKeyDown()
        {
            while (!Input.anyKeyDown)
            {
                
            }
            if (Input.inputString != "")
            {
                char tmp = Input.inputString[0];
                result = tmp;
            }
            yield return ' ';
        }

        /**
            * Run the interpreter with its given string
            */

        public void runBF()
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
                        try
                        {
                            result = ' ';
                            while (result == ' ')
                            {
                                StartCoroutine_Auto(GetAsciiOfKeyDown());
                            }
                            mem[mp] = result;
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
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
        }

        public void runOok()
        {
            while (ip < EOF)
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
                        try
                        {
                            result = ' ';
                            while (result == ' ')
                            {
                                StartCoroutine_Auto(GetAsciiOfKeyDown());
                            }
                            mem[mp] = result;
                        }
                        catch (Exception e)
                        {
                            Debug.LogException(e);
                        }
                        break;
                    case "Ook!Ook?":
                        if (mem[mp] == 0)
                        {
                            while (com[ip] != ']') ip++;
                        }
                        break;

                    case "Ook?Ook!":
                        if (mem[mp] != 0)
                        {
                            while (com[ip] != '[') ip--;
                        }
                        break;
                }

                // increment instruction mp
                ip += 2;
            }
        }
    }
}