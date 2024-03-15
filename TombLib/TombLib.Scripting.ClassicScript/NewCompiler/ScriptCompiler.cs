using System;
using System.Collections.Generic;
using System.IO;

namespace TombLib.Scripting.ClassicScript.NewCompiler;

public static class ScriptCompiler
{
	public static void CompileAll(string scriptDirectory, string outputDirectory, string baseLanguage = "English")
	{
		var logs = new List<string>();

		int[] totalStringCountArray;
		int totalFileCount;
		string[] extraNGTextElements = Array.Empty<string>();
		int[] extraNGIndices = Array.Empty<int>();
		string elementName;
		int index;
		string mainLanguage;
		string tempName;

		string scriptFilePath = Path.Combine(scriptDirectory, "Script.txt");
		string baseLanguageFilePath = Path.Combine(scriptDirectory, $"{baseLanguage}.txt");
		string datFilePath = Path.Combine(outputDirectory, "Script.dat");

		logs.Add("**** START COMPILING ****");
		logs.Add($"Removing old .dat files from: {outputDirectory}");

		foreach (string file in Directory.GetFiles(outputDirectory, "*.dat"))
		{
			File.Delete(file);
			logs.Add($"Removing: {file}");
		}

		if (!File.Exists(scriptFilePath))
			return;

		string scriptText = File.ReadAllText(scriptFilePath);

		logs.Add("Saving LANGUAGES.DAT ...");

		// Now save the English NG strings so you can check that in the language
		// Main are all present, otherwise give an error
		int stringCount = 0;
		var baseLanguageFile = new LanguageFile(baseLanguageFilePath);

		for (int i = 0; i <= baseLanguageFile.ExtraNGStrings.Count - 1; i++)
		{
			string[] oldVetEnglishText = (string[])extraNGTextElements.Clone();
			extraNGTextElements = new string[stringCount + 1];

			if (oldVetEnglishText != null)
				Array.Copy(oldVetEnglishText, extraNGTextElements, Math.Min(stringCount + 1, oldVetEnglishText.Length));

			int[] oldVetEnglishIndex = (int[])extraNGIndices.Clone();
			extraNGIndices = new int[stringCount + 1];

			if (oldVetEnglishIndex != null)
				Array.Copy(oldVetEnglishIndex, extraNGIndices, Math.Min(stringCount + 1, oldVetEnglishIndex.Length));

			extraNGTextElements[stringCount] = baseLanguageFile.ExtraNGStrings[i].Text;
			extraNGIndices[stringCount] = baseLanguageFile.ExtraNGStrings[i].Index;

			stringCount++;
		}

		//    var withBlock = BaseScriptDat;
		//    totalFileCount = 0;

		//    for (int i = 0; i <= withBlock.TotLanguage - 1; i++)
		//    {
		//        scriptFilePath = scriptDirectory + @"\" + withBlock.VetLanguage(i);
		//        datFilePath = CambiaEst(scriptFilePath, ".DAT");
		//        StampaMessaggio(Constants.vbTab + "Saving: " + datFilePath);

		//        if (File.Exists(scriptFilePath))
		//            AddErrore(0, false, GetStringa(250, Path.GetFileName(scriptFilePath)), withBlock.VetLanguage(i));
		//        else if (LeggeLanguageTxt(scriptFilePath))
		//        {
		//            // se questo linguaggio e' quello main
		//            // ed e' diverso da inglese
		//            // controllare che abbia tutte le strnghe extra ng
		//            // che aveva inglese
		//            elementName = Path.GetFileNameSenzaExt(scriptFilePath);

		//            if (elementName != "english" & elementName == Path.GetFileNameSenzaExt(mainLanguageDat))
		//            {
		//                for (int j = 0; j <= stringCount - 1; j++)
		//                {
		//                    index = extraNGIndices[j];
		//                    int k;

		//                    var withBlock1 = BaseLanguage;

		//                    for (k = 0; k <= withBlock1.TotNGExtra - 1; k++)
		//                    {
		//                        if (withBlock1.VetExtraStrings(k).Indice == index)
		//                            break;
		//                    }

		//                    if (k == withBlock1.TotNGExtra)
		//                    {
		//                        // non e' stata trovata
		//                        // ERROR: missing in ~1 file the Extra NG String corresponding to english ng string with index= ~2 and text = "~3"
		//                        AddErrore(-1, true, GetStringa(251, Path.GetFileName(scriptFilePath), Convert.ToString(index), extraNGTextElements[j]));
		//                        goto Attesa;
		//                    }
		//                }
		//            }

		//            if (ScriveLanguageDat(datFilePath))
		//            {
		//                var oldVetNomi = nameArray;
		//                nameArray = new string[totalFileCount + 1];

		//                // salvare dati
		//                if (oldVetNomi != null)
		//                    Array.Copy(oldVetNomi, nameArray, Math.Min(totalFileCount + 1, oldVetNomi.Length));

		//                var oldVetTotStrings = totalStringCountArray;
		//                totalStringCountArray = new int[totalFileCount + 1];

		//                if (oldVetTotStrings != null)
		//                    Array.Copy(oldVetTotStrings, totalStringCountArray, Math.Min(totalFileCount + 1, oldVetTotStrings.Length));

		//                nameArray[totalFileCount] = scriptFilePath;
		//                totalStringCountArray[totalFileCount] = BaseLanguage.TotStrings;
		//                totalFileCount++;
		//            }
		//        }
		//    }

		//    // verificare che non ci siano anomalie
		//    for (int i = 1; i <= totalFileCount - 1; i++)
		//    {
		//        if (totalStringCountArray[0] != totalStringCountArray[i])
		//            // WARNING: number of [Strings] is different in files: ~1 (~2) respect than ~3 (~4)
		//            AddErrore(-1, false, GetStringa(252, Path.GetFileName(nameArray[0]), Convert.ToString(totalStringCountArray[0]), Path.GetFileName(nameArray[i]), Convert.ToString(totalStringCountArray[i])), "");
		//    }

		//    // ora aggiornare i file anche in cartella precedente
		//    string initialDirectory = SoloDir(scriptDirectory);
		//    string mainScriptDat = Trim(MyPref.PathTrleNow) + @"\script.dat";

		//    if (File.Copy(scriptDirectory + @"\script.dat", mainScriptDat))
		//        StampaMessaggio("Created: " + mainScriptDat);
		//    else
		//        // ERROR copying script.dat in folder:
		//        AddErrore(-1, true, GetStringa(253) + initialDirectory);

		//    if (File.Copy(scriptDirectory + @"\" + Path.GetFileName(mainLanguageDat), mainLanguageDat))
		//        StampaMessaggio("Created: " + mainLanguageDat);
		//    else
		//        // ERROR copying ~1 in folder:
		//        AddErrore(-1, true, GetStringa(254, Path.GetFileName(mainLanguageDat)) + initialDirectory);
		//    // COMPLETED
		//    StampaMessaggio("******** " + GetStringa(255) + " ********");
		//    StampaLog("Completed compilation");

		//Attesa:
		//    if (TestAttendiFine & Glob.TestCompNoWait)
		//    {
		//        StampaLog("Wait the escape key");
		//        TestPremutoTasto = false;
		//        StampaMessaggio("PRESS [ESC] KEY TO CLOSE THE WINDOW ...");

		//        // qui devo attendere e come faccio?
		//        while (TestPremutoTasto == false)
		//            DoEvents();
		//    }

		//    return;

		//Errore:
		//    StampaLog("Error in  CompilaTutto(): " + Information.Err.Description);
		//    CallMsgBox("Internal error: " + Information.Err.Description, Constants.vbCritical);
		//    TestPremutoTasto = true;
	}
}
