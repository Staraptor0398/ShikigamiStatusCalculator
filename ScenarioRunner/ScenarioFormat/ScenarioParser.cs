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
			List<string> tokens = tokenize(line);

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
			else if (matches(tokens, "LOAD", "MITAMA"))
			{
				commandType = ScenarioCommandType.LOAD_MITAMA;
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
			else if (matches(tokens, "WAIT", "SHIKIGAMI", "AUTO", "REPAIR"))
			{
				commandType = ScenarioCommandType.WAIT_SHIKIGAMI_AUTO_REPAIR;
				argumentStartIndex = 4;
			}
			else
			{
				throw new FormatException($"Unknown command at line {lineNumber}: {line}");
			}

			return new ScenarioStep(lineNumber, commandType, getArguments(tokens, argumentStartIndex), rawText);
		}

		private List<string> tokenize(string line)
		{
			var tokens = new List<string>();
			var token = new StringBuilder();
			bool inQuotedString = false;

			for (int i = 0; i < line.Length; i++)
			{
				char c = line[i];

				if (c == '"')
				{
					inQuotedString = !inQuotedString;
					continue;
				}

				if (char.IsWhiteSpace(c) && !inQuotedString)
				{
					if (token.Length > 0)
					{
						tokens.Add(token.ToString());
						token.Clear();
					}

					continue;
				}

				token.Append(c);
			}

			if (inQuotedString)
			{
				throw new FormatException("Quoted string is not closed.");
			}

			if (token.Length > 0)
			{
				tokens.Add(token.ToString());
			}

			return tokens;
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
