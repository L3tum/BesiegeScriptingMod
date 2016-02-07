using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BesiegeScriptingMod
{
    class BrainfuckBehaviour : MonoBehaviour
    {
        // Instance info.
        // Instance of Brainfuck interpreter, actually handles interpreting.

        /** The array that acts as the interpreter's virtual memory */
        private char[] mem;
        /** The memory pointer */
        private int mp;

        /** The string containing the comands to be executed */
        private char[] com;

        /** The instruction pointer */
        private int ip = 0;

        private int EOF; //End Of File

        public String sauce = "";

        public char result = ' ';

        /**
            * Create the Brainfuck VM and give it the string to be interpreted.
            * @param s The string to be interpreted.
            */

        public BrainfuckBehaviour(string code)
        {
            mem = new char[30000];
            mp = 0;
            com = code.ToCharArray();
            EOF = com.Length;
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

        public void run()
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
                            Debug.Log(e.StackTrace);
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
    }
}