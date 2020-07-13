using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblerN2T
{
    public class Instruction
    {
        public string instruction;
        public long linenumber;
    }

    public static class AssemblerSpecs
    {
        public static IDictionary<string, int> PreSymbols = new Dictionary<string, int>()
        {
            { "R0" , 0 },
            { "R1" , 1 },
            { "R2" , 2 },
            { "R3" , 3 },
            { "R4" , 4 },
            { "R5" , 5 },
            { "R6" , 6 },
            { "R7" , 7 },
            { "R8" , 8 },
            { "R9" , 9 },
            { "R10" , 10 },
            { "R11" , 11 },
            { "R12" , 12 },
            { "R13" , 13 },
            { "R14" , 14 },
            { "R15" , 15 },
            { "SP" , 0 },
            { "LCL" , 1 },
            { "ARG" , 2 },
            { "THIS" , 3 },
            { "THAT" , 4 },
            { "SCREEN" , 16384 },
            { "KBD" , 24576 },
        };
        public static IDictionary<string, string> DEST = new Dictionary<string, string>()
        {
            { "null" , "000" },
            { "M" , "001" },
            { "D" , "010" },
            { "MD" , "011" },
            { "A" , "100" },
            { "AM" , "101" },
            { "AD" , "110" },
            { "AMD" , "111" },
        };
        public static IDictionary<string, string> COMP = new Dictionary<string, string>()
        {
            { "0" , "0101010" },
            { "1" , "0111111" },
            { "-1" , "0111010" },
            { "D" , "0001100" },
            { "A" , "0110000" },
            { "M" , "1110000" },
            { "!D" , "0001101" },
            { "!A" , "0110001" },
            { "!M" , "1110001" },
            { "-D" , "0001111" },
            { "-A" , "0110011" },
            { "-M" , "1110011" },
            { "D+1" , "0011111" },
            { "A+1" , "0110111" },
            { "M+1" , "1110111" },
            { "D-1" , "0001110" },
            { "A-1" , "0110010" },
            { "M-1" , "1110010" },
            { "D+A" , "0000010" },
            { "D+M" , "1000010" },
            { "D-A" , "0010011" },
            { "D-M" , "1010011" },
            { "A-D" , "0000111" },
            { "M-D" , "1000111" },
            { "D&A" , "0000000" },
            { "D&M" , "1000000" },
            { "D|A" , "0010101" },
            { "D|M" , "1010101" },
        };
        public static IDictionary<string, string> JUMP = new Dictionary<string, string>()
        {
            { "null" , "000" },
            { "JGT" , "001" },
            { "JEQ" , "010" },
            { "JGE" , "011" },
            { "JLT" , "100" },
            { "JNE" , "101" },
            { "JLE" , "110" },
            { "JMP" , "111" },
        };
        public static List<Instruction> CleanCode(string codeText)
        {
            var OutList = new List<Instruction>();
            var Codelines = codeText.Split('\n');
            for (long i = 0; i < Codelines.Length; i++)
            {
                var Line = Codelines[i].Replace("\r", "").Replace(" ", "").Split(new char[] { '/', '/' });
                if (!string.IsNullOrEmpty(Line?.FirstOrDefault()))
                    OutList.Add(new Instruction() { instruction = Line.FirstOrDefault(), linenumber = i });
            }

            return OutList;
        }
        public static List<string> Assemble(string Text)
        {
            var HackCode = new List<string>();
            var Instructions = AssemblerSpecs.CleanCode(Text);
            var SymbolTable = new Dictionary<string, int>(AssemblerSpecs.PreSymbols);

            for (int i = 0; i < Instructions.Count; i++)
            {
                var instruction = Instructions[i].instruction;
                var instructionLNo = Instructions[i].linenumber;

                if (instruction.StartsWith("("))
                {
                    var label = instruction?.Replace("(", "")?.Replace(")", "")?.Trim();
                    if (string.IsNullOrEmpty(label))
                        throw new Exception("Invalid Label at line " + instructionLNo + 1);
                    SymbolTable.Add(label, i);
                    Instructions.RemoveAt(i--);
                }
            }

            int lastVar = 16;
            for (int i = 0; i < Instructions.Count; i++)
            {
                var instruction = Instructions[i].instruction;
                var instructionLNo = Instructions[i].linenumber;

                if (instruction.StartsWith("@"))
                {
                    var label = instruction?.Replace("@", "")?.Trim();
                    if (string.IsNullOrEmpty(label))
                        throw new Exception("Invalid address at line " + (instructionLNo + 1));
                    int address = 0;
                    if (int.TryParse(label, out address))
                        continue;
                    if (SymbolTable.ContainsKey(label))
                        address = SymbolTable[label];
                    else
                    {
                        SymbolTable[label] = lastVar++;
                        address = SymbolTable[label];
                    }
                }
            }

            for (int i = 0; i < Instructions.Count; i++)
            {
                var instruction = Instructions[i].instruction;
                var instructionLNo = Instructions[i].linenumber;

                try
                {
                    if (instruction.StartsWith("@"))
                    {
                        var label = instruction?.Replace("@", "")?.Trim();
                        int address;
                        if (!int.TryParse(label, out address))
                        {
                            if (SymbolTable.ContainsKey(label))
                                address = SymbolTable[label];
                            else
                                throw new Exception("'" + label + "' label not found at line number " + instructionLNo);
                        }
                        char[] hack = new char[16] { '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0' };
                        var stringAddres = Convert.ToString(address, 2);
                        for (int j = 16 - stringAddres.Length; j < 16; j++)
                        {
                            hack[j] = stringAddres[j - (16 - stringAddres.Length)];
                        }
                        HackCode.Add(new string(hack));
                    }
                    else
                    {
                        string dest = "null";
                        string comp = "";
                        string jump = "null";
                        if (instruction.Contains(';') && instruction.Contains('='))
                        {
                            var split_destAndOther = instruction.Split('=');
                            var split_compAndJump = split_destAndOther[1].Split(';');

                            dest = split_destAndOther[0];
                            comp = split_compAndJump[0];
                            jump = split_compAndJump[1];
                        }
                        else if (instruction.Contains(';'))
                        {
                            var splited = instruction.Split(';');
                            comp = splited[0];
                            jump = splited[1];
                        }
                        else if (instruction.Contains('='))
                        {
                            var splited = instruction.Split('=');
                            dest = splited[0];
                            comp = splited[1];
                        }
                        else
                        {
                            throw new Exception("Invalid instruction line at " + instructionLNo);
                        }
                        var hack = "111" + COMP[comp] + DEST[dest] + JUMP[jump];
                        HackCode.Add(hack);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + " line at " + instructionLNo);
                }
            }
            return HackCode;
        }
    }
}
