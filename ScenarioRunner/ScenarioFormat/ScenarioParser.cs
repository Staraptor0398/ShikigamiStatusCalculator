using System;
using System.Collections.Generic;
using System.Text;

namespace ScenarioRunner.ScenarioFormat
{
	public class ScenarioParser
	{
		public Scenario Parse(string filePath, IReadOnlyList<string> lines)
		{
			int startLine = -1;
			int endLine = -1;
			var steps = new List<ScenarioStep>();

			for (int i = 0; i < lines.Count; i++)
			{
				int lineNumber = i + 1;
				string rawText = lines[i];
				string line = rawText.Trim();

				if (string.IsNullOrWhiteSpace(line))
				{
					continue;
				}

				if (line.StartsWith("#"))
				{
					continue;
				}

				if (line == "START")
				{
					if (startLine != -1)
					{
						throw new FormatException($"START is duplicated at line {lineNumber}.");
					}

					startLine = lineNumber;
					continue;
				}

				if (line == "END")
				{
					if (endLine != -1)
					{
						throw new FormatException($"END is duplicated at line {lineNumber}.");
					}

					endLine = lineNumber;
					continue;
				}

				steps.Add(parseStep(lineNumber, rawText, line));
			}

			return new Scenario(filePath, startLine, endLine, steps);
		}

		private ScenarioStep parseStep(int lineNumber, string rawText, string line)
		{
			tokenize(lineNumber, line, out List<string> tokens, out List<bool> quotedTokens);

			if (tokens.Count == 0)
			{
				throw new FormatException($"Command is empty at line {lineNumber}.");
			}

			ScenarioCommandType commandType;
			int argumentStartIndex;

			if (matches(tokens, "LAUNCH", "GUI"))
			{
				commandType = ScenarioCommandType.LAUNCH_GUI;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "OPEN", "GUI"))
			{
				commandType = ScenarioCommandType.OPEN_GUI;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "CLOSE", "GUI"))
			{
				commandType = ScenarioCommandType.CLOSE_GUI;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "CLOSE", "DIALOG"))
			{
				commandType = ScenarioCommandType.CLOSE_DIALOG;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "SEL", "SHIKIGAMI"))
			{
				commandType = ScenarioCommandType.SELECT_SHIKIGAMI;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "EQUIP", "MITAMA"))
			{
				commandType = ScenarioCommandType.EQUIP_MITAMA;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "LOAD", "MITAMA"))
			{
				commandType = ScenarioCommandType.LOAD_MITAMA;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "COMPARE", "SNAPSHOT"))
			{
				commandType = ScenarioCommandType.COMPARE_SNAPSHOT;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "CALC"))
			{
				commandType = ScenarioCommandType.CALCULATE;
				argumentStartIndex = 1;
			}
			else if (matches(tokens, "CLEAR"))
			{
				commandType = ScenarioCommandType.CLEAR;
				argumentStartIndex = 1;
			}
			else if (matches(tokens, "RELOAD", "SHIKIGAMI"))
			{
				commandType = ScenarioCommandType.RELOAD_SHIKIGAMI;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "BREAK", "SHIKIGAMI", "HEADER"))
			{
				commandType = ScenarioCommandType.BREAK_SHIKIGAMI_HEADER;
				argumentStartIndex = 3;
			}
			else if (matches(tokens, "REMOVE", "SHIKIGAMI"))
			{
				commandType = ScenarioCommandType.REMOVE_SHIKIGAMI;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "CREATE", "SHIKIGAMI", "BACKUP"))
			{
				commandType = ScenarioCommandType.CREATE_SHIKIGAMI_BACKUP;
				argumentStartIndex = 3;
			}
			else if (matches(tokens, "RECOVER", "SHIKIGAMI"))
			{
				commandType = ScenarioCommandType.RECOVER_SHIKIGAMI;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "CHECK", "CALC"))
			{
				commandType = ScenarioCommandType.CHECK_CALCULATION;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "CHECK", "SHIKIGAMI"))
			{
				commandType = ScenarioCommandType.CHECK_SHIKIGAMI;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "CHECK", "DIALOG"))
			{
				commandType = ScenarioCommandType.CHECK_DIALOG;
				argumentStartIndex = 2;
			}
			else if (matches(tokens, "CHECK", "SNAPSHOT", "COMPARISON"))
			{
				commandType = ScenarioCommandType.CHECK_SNAPSHOT_COMPARISON;
				argumentStartIndex = 3;
			}
			else if (matches(tokens, "WAIT", "SHIKIGAMI", "AUTO", "REPAIR"))
			{
				commandType = ScenarioCommandType.WAIT_SHIKIGAMI_AUTO_REPAIR;
				argumentStartIndex = 4;
			}
			else
			{
				throw new FormatException($"Unknown command at line {lineNumber}: {line}");
			}

			validateQuotedArguments(lineNumber, rawText, commandType, tokens, quotedTokens);

			return new ScenarioStep(lineNumber, commandType, getArguments(tokens, argumentStartIndex), rawText);
		}

		private void tokenize(int lineNumber, string line, out List<string> tokens, out List<bool> quotedTokens)
		{
			tokens = new List<string>();
			quotedTokens = new List<bool>();

			var token = new StringBuilder();
			bool inQuotedString = false;
			bool hasToken = false;
			bool tokenIsQuoted = false;
			bool quoteClosed = false;

			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];

				if (c == '"')
				{
					if (!inQuotedString)
					{
						if (hasToken)
						{
							tokenIsQuoted = false;
						}
						else
						{
							tokenIsQuoted = true;
						}

						inQuotedString = true;
						hasToken = true;
						quoteClosed = false;
					}
					else
					{
						inQuotedString = false;
						quoteClosed = true;
					}

					continue;
				}

				if (char.IsWhiteSpace(c) && !inQuotedString)
				{
					if (hasToken)
					{
						tokens.Add(token.ToString());
						quotedTokens.Add(tokenIsQuoted);

						token.Clear();
						hasToken = false;
						tokenIsQuoted = false;
						quoteClosed = false;
					}

					continue;
				}

				if (!inQuotedString && quoteClosed)
				{
					tokenIsQuoted = false;
				}

				token.Append(c);
				hasToken = true;
			}

			if (inQuotedString)
			{
				throw new FormatException($"Quoted string is not closed at line {lineNumber}.");
			}

			if (hasToken)
			{
				tokens.Add(token.ToString());
				quotedTokens.Add(tokenIsQuoted);
			}
		}

		private void validateQuotedArguments(int lineNumber, string rawText, ScenarioCommandType commandType, IReadOnlyList<string> tokens, IReadOnlyList<bool> quotedTokens)
		{
			switch (commandType)
			{
				case ScenarioCommandType.SELECT_SHIKIGAMI:
					requireQuotedToken(lineNumber, rawText, quotedTokens, 2);
					break;

				case ScenarioCommandType.EQUIP_MITAMA:
					validateEquipMitamaQuotedArguments(lineNumber, rawText, tokens, quotedTokens);
					break;

				case ScenarioCommandType.LOAD_MITAMA:
					requireQuotedToken(lineNumber, rawText, quotedTokens, 2);
					break;

				case ScenarioCommandType.COMPARE_SNAPSHOT:
					requireQuotedToken(lineNumber, rawText, quotedTokens, 2);
					requireQuotedToken(lineNumber, rawText, quotedTokens, 3);
					break;

				case ScenarioCommandType.REMOVE_SHIKIGAMI:
					requireQuotedToken(lineNumber, rawText, quotedTokens, 2);
					break;

				case ScenarioCommandType.CHECK_DIALOG:
					requireQuotedToken(lineNumber, rawText, quotedTokens, 2);
					break;

				case ScenarioCommandType.CHECK_SNAPSHOT_COMPARISON:
					requireQuotedToken(lineNumber, rawText, quotedTokens, 3);
					requireQuotedToken(lineNumber, rawText, quotedTokens, 4);
					break;
			}
		}

		private void validateEquipMitamaQuotedArguments(int lineNumber, string rawText, IReadOnlyList<string> tokens, IReadOnlyList<bool> quotedTokens)
		{
			if (tokens.Count <= 2)
			{
				return;
			}

			switch (tokens[2])
			{
				case "MAIN":
					requireQuotedToken(lineNumber, rawText, quotedTokens, 4);
					break;

				case "SUB":
					requireQuotedToken(lineNumber, rawText, quotedTokens, 5);
					requireQuotedToken(lineNumber, rawText, quotedTokens, 6);
					break;

				case "SET":
				case "UNIQUE":
					requireQuotedToken(lineNumber, rawText, quotedTokens, 4);
					break;
			}
		}

		private void requireQuotedToken(int lineNumber, string rawText, IReadOnlyList<bool> quotedTokens, int tokenIndex)
		{
			if (tokenIndex >= quotedTokens.Count)
			{
				return;
			}

			if (!quotedTokens[tokenIndex])
			{
				throw new FormatException($"String argument must be enclosed in double quotes at line {lineNumber}: {rawText}");
			}
		}

		private bool matches(IReadOnlyList<string> tokens, params string[] commandTokens)
		{
			if (tokens.Count < commandTokens.Length)
			{
				return false;
			}

			for (int i = 0; i < commandTokens.Length; i++)
			{
				if (tokens[i] != commandTokens[i])
				{
					return false;
				}
			}

			return true;
		}

		private IReadOnlyList<string> getArguments(IReadOnlyList<string> tokens, int startIndex)
		{
			var arguments = new List<string>();

			for (int i = startIndex; i < tokens.Count; i++)
			{
				arguments.Add(tokens[i]);
			}

			return arguments;
		}
	}
}
