using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShikigamiDataAuditor.DataGeneration
{
	public class MoegirlNameResolver
	{
		private const string PAGE_TITLE_PREFIX = "阴阳师手游:";

		private static readonly IReadOnlyDictionary<string, string> NAME_ALIASES = new Dictionary<string, string>(StringComparer.Ordinal)
		{
			{ "因幡かぐや姫", "因幡辉夜姬" },
			{ "天剣刃心鬼切", "天剑韧心鬼切" },
			{ "尋森シシオ", "寻森小鹿男" },
			{ "本真三尾の狐", "本真三尾狐" },
			{ "灼華桃の精", "灼华桃花妖" },
			{ "神堕オロチ", "神堕八岐大蛇" },
			{ "稲荷御饌津", "稻荷神御馔津" },
			{ "絵世花鳥風月", "绘世花鸟卷" },
			{ "遥念煙々羅", "遥念烟烟罗" },
			{ "雲上不見岳", "云间不见岳" },
			{ "願紡縁結神", "纺愿缘结神" },
			{ "驍浪荒川の主", "骁浪荒川之主" },
			{ "夢引き胡蝶の精", "梦引蝴蝶精" },
			{ "招福座敷童子", "福悦座敷童子" },
			{ "暁日恵比寿", "晨晖惠比寿" },
			{ "盈虚千姫", "鲸汐千姬" },
			{ "神酒星熊童子", "神酿星熊童子" },

			{ "かぐや姫", "辉夜姬" },
			{ "イザナミ", "伊邪那美" },
			{ "オロチ", "八岐大蛇" },
			{ "シシオ", "小鹿男" },
			{ "一目連", "一目连" },
			{ "両面仏", "两面佛" },
			{ "妖刀姫", "妖刀姬" },
			{ "花鳥風月", "花鸟卷" },
			{ "荒川の主", "荒川之主" },
			{ "酒呑童子", "酒吞童子" },
			{ "昆沙門天", "毗沙门天" },
			{ "餓者髑髏", "荒骷髅" },

			{ "キョンシー兄", "跳跳哥哥" },
			{ "傀儡師", "傀儡师" },
			{ "匣の少女", "匣中少女" },
			{ "夢喰い", "食梦貘" },
			{ "書妖", "书翁" },
			{ "桃の精", "桃花妖" },
			{ "桜の精", "樱花妖" },
			{ "棋聖", "弈" },
			{ "煙々羅", "烟烟罗" },
			{ "猫又", "猫掌柜" },
			{ "白無常", "鬼使白" },
			{ "納棺師", "入殓师" },
			{ "薫", "熏" },
			{ "黒無常", "鬼使黑" },
			{ "御明かし", "慧明灯" },
			{ "海の蝶", "灵海蝶" },
			{ "白粉婆", "粉婆婆" }
		};

		private static readonly IReadOnlyDictionary<char, char> SIMPLIFIED_CHARACTER_MAP = new Dictionary<char, char>
		{
			{ '見', '见' },
			{ '葉', '叶' },
			{ '嶽', '岳' },
			{ '滝', '泷' },
			{ '姫', '姬' },
			{ '獲', '获' },
			{ '鳥', '鸟' },
			{ '羅', '罗' },
			{ '絡', '络' },
			{ '婦', '妇' },
			{ '風', '风' },
			{ '華', '华' },
			{ '驍', '骁' },
			{ '雲', '云' },
			{ '絵', '绘' },
			{ '願', '愿' },
			{ '縁', '缘' },
			{ '結', '结' },
			{ '煙', '烟' },
			{ '稲', '稻' },
			{ '剣', '剑' },
			{ '暁', '晓' },
			{ '恵', '惠' },
			{ '燈', '灯' },
			{ '魚', '鱼' },
			{ '無', '无' },
			{ '書', '书' },
			{ '桜', '樱' },
			{ '聖', '圣' },
			{ '納', '纳' },
			{ '黒', '黑' },
			{ '両', '两' }
		};

		public IReadOnlyList<string> ResolveCandidates(string shikigamiName)
		{
			if (string.IsNullOrWhiteSpace(shikigamiName))
			{
				return Array.Empty<string>();
			}

			List<string> candidates = new List<string>();

			string normalizedName = shikigamiName.Trim();
			string alias;

			if (NAME_ALIASES.TryGetValue(normalizedName, out alias))
			{
				addCandidate(candidates, alias);
			}

			addCandidate(candidates, convertToSimplifiedCharacters(normalizedName));
			addCandidate(candidates, normalizedName);

			return candidates
				.Select(name => PAGE_TITLE_PREFIX + name)
				.ToList();
		}

		private static void addCandidate(ICollection<string> candidates, string candidate)
		{
			if (string.IsNullOrWhiteSpace(candidate))
			{
				return;
			}

			if (candidates.Contains(candidate))
			{
				return;
			}

			candidates.Add(candidate);
		}

		private static string convertToSimplifiedCharacters(string name)
		{
			StringBuilder builder = new StringBuilder(name.Length);

			foreach (char character in name)
			{
				char convertedCharacter;

				if (SIMPLIFIED_CHARACTER_MAP.TryGetValue(character, out convertedCharacter))
				{
					builder.Append(convertedCharacter);
				}
				else
				{
					builder.Append(character);
				}
			}

			return builder.ToString();
		}
	}
}
