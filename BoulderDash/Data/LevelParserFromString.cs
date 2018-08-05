#define OOP
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using BoulderDashGUI.Model;
using BoulderDashGUI.Model.GameObjects;
using BoulderDashGUI.Model.GameObjects.Diamonds;
using BoulderDashGUI.Model.Monsters;

namespace BoulderDashGUI.Data
{
    class LevelParserFromString
    {
        private static LevelParserFromString _instance;
        public static LevelParserFromString Instance => _instance;

        static LevelParserFromString()
        {
            _instance = new LevelParserFromString();
        }

        private readonly Dictionary<char, GameObject> _gameObj;

        private LevelParserFromString()
        {
            _gameObj = new Dictionary<char, GameObject>
            {
                ['.'] = new Sand(),
                ['#'] = new Wall(),
                ['_'] = new Empty(),
                ['S'] = new Stone(),
                ['D'] = new CommonDiamond(),
                ['E'] = new Exit(),
                ['I'] = Player.Instance
            };
        }

        public KeyValuePair<GameObject[,], int> ParseFromFile(string fileName)
        {
            var lineCount = File.ReadLines(fileName).Count();

            var linesLength = -1;
            using (var fs = new StreamReader(fileName))
            {
                fs.ReadLine();
                var line = fs.ReadLine();

                if (line is null) return new KeyValuePair<GameObject[,], int>();

                linesLength = line.Length;
            }
            var lvl = new GameObject[lineCount - 1, linesLength];

            int diamondsToNextLvl;
            using (var fs = new StreamReader(fileName))
            {
                var isPlayerDefined = false;
                diamondsToNextLvl = Int32.Parse(fs.ReadLine() ??
                                                throw new DataException(
                                                    "1st row in file should be amount of diamonds to next lvl"));
                var row = 0;
                while (true)
                {
                    // Читаем строку из файла во временную переменную.
                    var lvlString = fs.ReadLine();

                    // Если достигнут конец файла, прерываем считывание.
                    if (lvlString is null) break;

                    for (int i = 0, col = 0; i < lvlString.Length; i++)
                    {
                        if (lvlString[i] == '\r' || lvlString[i] == '\n') continue;

                        // ['#'] = new Wall(),
                        // ['_'] = new Empty(),
                        // ['S'] = new Stone(),
                        // ['D'] = new CommonDiamond(),
                        // ['E'] = new Exit(),
                        // ['I'] = Player.Instance

                        GameObject newGameObject = null;
                        switch (lvlString[i])
                        {
                            case '.':
                                newGameObject = new Sand();
                                break;
                            case '#':
                                newGameObject = new Wall();
                                break;
                            case '_':
                                newGameObject = new Empty();
                                break;
                            case 'S':
                                newGameObject = new Stone();
                                break;
                            case 'D':
                                newGameObject = new CommonDiamond();
                                break;
                            case 'B':
                                newGameObject = new Butterfly(row, col);
                                break;
                            case 'E':
                                newGameObject = new Exit();
                                break;
                            case 'I':
                                if (isPlayerDefined)
                                    throw new DataException("Player is already defined");
                                newGameObject = Player.Instance;
                                isPlayerDefined = true;
                                break;
                        }

                        lvl[row, col++] = newGameObject ?? throw new DataException($"Boulder dash hasn't got value for symbol {lvlString[i]} at position {i}");
                    }
                    row++;
                }

            }

            return new KeyValuePair<GameObject[,], int>(lvl, diamondsToNextLvl);
        }
    }
}
