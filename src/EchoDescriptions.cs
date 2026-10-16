using System;
using System.Collections.Generic;
using RoR2;

namespace AcolyteBetterEcho
{
    /// <summary>
    /// Extends the vanilla Anima Echo and Sacrifice descriptions with the mod's changes,
    /// in every language the game ships. The vanilla text is kept and the additions appended,
    /// using each language's own terms for Conviction, Litany and Anima Echo.
    /// </summary>
    internal static class EchoDescriptions
    {
        private const string EchoDescriptionToken = "ACOLYTE_SPECIAL_ALT_TRANSFORM_DESCRIPTION";
        private const string SacrificeDescriptionToken = "ACOLYTE_SPECIAL_DESCRIPTION";

        private struct Additions
        {
            // {0} = Convictions per Sacrifice, {1} = Russian plural of "заряд"
            public string echo;
            public string sacrifice;
        }

        // Keyed by the game's language folder name
        private static readonly Dictionary<string, Additions> additions = new Dictionary<string, Additions>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = new Additions
            {
                echo = " Activating it <style=cIsUtility>resets all skill cooldowns</style>. Convictions earned during the Litany are <style=cIsUtility>stored</style> and granted when it ends.",
                sacrifice = " With Anima Echo, gain <style=cIsDamage>{0}</style> Convictions instead.",
            },
            ["FR"] = new Additions
            {
                echo = " L'activation <style=cIsUtility>réinitialise le temps de recharge de toutes vos compétences</style>. Les Convictions obtenues pendant la Litanie sont <style=cIsUtility>conservées</style> et accordées à la fin de celle-ci.",
                sacrifice = " Avec l'Écho d'anima, confère <style=cIsDamage>{0}</style> Convictions à la place.",
            },
            ["IT"] = new Additions
            {
                echo = " L'attivazione <style=cIsUtility>azzera la ricarica di tutte le tue abilità</style>. Le Convinzioni ottenute durante la Litania vengono <style=cIsUtility>conservate</style> e conferite al suo termine.",
                sacrifice = " Con l'Eco dell'Anima, ottieni invece <style=cIsDamage>{0}</style> Convinzioni.",
            },
            ["RU"] = new Additions
            {
                echo = " При активации <style=cIsUtility>сбрасывает время восстановления всех навыков</style>. Заряды убеждения, полученные во время Литании, <style=cIsUtility>накапливаются</style> и начисляются после её окончания.",
                sacrifice = " С Эхом Анимы даёт <style=cIsDamage>{0}</style> {1} убеждения.",
            },
            ["de"] = new Additions
            {
                echo = " Beim Aktivieren werden <style=cIsUtility>die Abklingzeiten aller deiner Fähigkeiten zurückgesetzt</style>. Während der Litanei erhaltene Überzeugungen werden <style=cIsUtility>gespeichert</style> und nach ihrem Ende gewährt.",
                sacrifice = " Mit Anima-Echo erhältst du stattdessen <style=cIsDamage>{0}</style> Überzeugungen.",
            },
            ["es-ES"] = new Additions
            {
                echo = " Al activarlo, <style=cIsUtility>se reinician los tiempos de reutilización de todas tus habilidades</style>. Las convicciones obtenidas durante la Letanía se <style=cIsUtility>acumulan</style> y se otorgan al terminar.",
                sacrifice = " Con Eco de almas, obtiene <style=cIsDamage>{0}</style> convicciones en su lugar.",
            },
            ["es-419"] = new Additions
            {
                echo = " Al activarlo, <style=cIsUtility>se reinician los tiempos de reutilización de todas tus habilidades</style>. Las convicciones obtenidas durante la Letanía se <style=cIsUtility>acumulan</style> y se otorgan al terminar.",
                sacrifice = " Con Eco de almas, obtiene <style=cIsDamage>{0}</style> convicciones en su lugar.",
            },
            ["pt-BR"] = new Additions
            {
                echo = " Ao ativar, <style=cIsUtility>reinicia a recarga de todas as suas habilidades</style>. Convicções obtidas durante a Litania são <style=cIsUtility>armazenadas</style> e concedidas quando ela termina.",
                sacrifice = " Com Eco de Anima, ganha <style=cIsDamage>{0}</style> Convicções em vez disso.",
            },
            ["tr"] = new Additions
            {
                echo = " Etkinleştirildiğinde <style=cIsUtility>tüm yeteneklerinin bekleme sürelerini sıfırlar</style>. Ayin sırasında kazanılan İnançlar <style=cIsUtility>saklanır</style> ve Ayin sona erdiğinde verilir.",
                sacrifice = " Anima Yankısı seçiliyken bunun yerine <style=cIsDamage>{0}</style> İnanç kazanırsın.",
            },
            ["ja"] = new Additions
            {
                echo = "発動すると<style=cIsUtility>すべてのスキルのクールダウンがリセットされる</style>。リタニー中に獲得したコンヴィクションは<style=cIsUtility>蓄積され</style>、リタニー終了時に付与される。",
                sacrifice = "アニマの残響を選択している場合、代わりに<style=cIsDamage>{0}</style> 個のコンヴィクションを獲得する。",
            },
            ["ko"] = new Additions
            {
                echo = " 발동 시 <style=cIsUtility>모든 스킬의 재사용 대기시간이 초기화</style>됩니다. 호칭기도 중에 얻은 신념은 <style=cIsUtility>저장</style>되었다가 호칭기도가 끝나면 지급됩니다.",
                sacrifice = " 생령의 메아리 선택 시 대신 신념 <style=cIsDamage>{0}</style>중첩을 얻습니다.",
            },
            ["zh-CN"] = new Additions
            {
                echo = "发动时<style=cIsUtility>重置所有技能的冷却时间</style>。诵祷期间获得的信念会被<style=cIsUtility>储存</style>，并在诵祷结束后获得。",
                sacrifice = "选择灵魂回响时，改为获得<style=cIsDamage>{0}</style>层信念。",
            },
        };

        internal static void Init()
        {
            Language.onCurrentLanguageChanged += Apply;
            Apply();
        }

        private static void Apply()
        {
            Language language = Language.currentLanguage;
            if (language == null || !additions.TryGetValue(Language.currentLanguageName ?? "", out Additions text))
            {
                return;
            }

            Append(language, EchoDescriptionToken, text.echo);
            int convictions = AcolyteBetterEchoPlugin.EchoSacrificeConvictions.Value;
            // Vanilla Sacrifice grants 2, so only mention a different value
            if (convictions != 2)
            {
                Append(language, SacrificeDescriptionToken, string.Format(text.sacrifice, convictions, RussianChargeWord(convictions)));
            }
        }

        private static void Append(Language language, string token, string addition)
        {
            string current = language.GetLocalizedStringByToken(token);
            // Strings are reloaded on language change, but guard against appending twice
            if (!current.EndsWith(addition, StringComparison.Ordinal))
            {
                language.SetStringByToken(token, current + addition);
            }
        }

        /// <summary>Russian plural of "заряд" for the given count.</summary>
        private static string RussianChargeWord(int count)
        {
            int mod100 = count % 100;
            int mod10 = count % 10;
            if (mod100 >= 11 && mod100 <= 14)
            {
                return "зарядов";
            }
            if (mod10 == 1)
            {
                return "заряд";
            }
            if (mod10 >= 2 && mod10 <= 4)
            {
                return "заряда";
            }
            return "зарядов";
        }
    }
}
