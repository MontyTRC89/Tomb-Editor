using TombLib.Scripting.ClassicScript;
using TombLib.Scripting.ClassicScript.Diagnostics;
using TombLib.Scripting.ClassicScript.Hover;
using TombLib.Scripting.ClassicScript.Mnemonics;
using TombLib.Scripting.ClassicScript.Navigation;
using TombLib.Scripting.ClassicScript.Services;
using TombLib.Scripting.ClassicScript.Signatures;
using TombLib.Scripting.ClassicScript.Syntaxes;
using TombLib.Scripting.GameFlowScript;
using TombLib.Scripting.GameFlowScript.Completion;
using TombLib.Scripting.GameFlowScript.Hover;
using TombLib.Scripting.GameFlowScript.Navigation;
using TombLib.Scripting.GameFlowScript.Services;
using TombLib.Scripting.TRX;
using TombLib.Scripting.TRX.Completion;
using TombLib.Scripting.TRX.Hover;
using TombLib.Scripting.TRX.Navigation;
using TombLib.Scripting.TRX.Services;

namespace TombEditor.Tests.ScriptingStudio;

internal static class ScriptingLanguageServicesTestFactory
{
    public static ClassicScriptLanguageServices CreateClassicScript()
    {
        var lineService = new ClassicScriptLineService();
        var mnemonicCatalogService = new ClassicScriptMnemonicCatalogService();
        var syntaxCatalogService = new ClassicScriptSyntaxCatalogService();
        var commandService = new ClassicScriptCommandService(lineService, mnemonicCatalogService, syntaxCatalogService);
        var indexService = new ClassicScriptIndexService(commandService, lineService, mnemonicCatalogService);
        var errorDetector = new ErrorDetector(lineService, commandService, syntaxCatalogService);

        return new ClassicScriptLanguageServices(
            new ClassicScriptDefinitionProvider(commandService),
            new ClassicScriptHoverProvider(lineService, commandService, mnemonicCatalogService),
            new ClassicScriptSignatureHelpProvider(commandService),
            errorDetector,
            lineService,
            commandService,
            indexService);
    }

    public static GameFlowLanguageServices CreateGameFlowScript()
    {
        var lineService = new GameFlowScriptLineService();
        var documentService = new GameFlowScriptDocumentService(lineService);

        return new GameFlowLanguageServices(
            new GameFlowDefinitionProvider(documentService),
            new GameFlowHoverProvider(),
            new GameFlowCompletionProvider(),
            lineService,
            documentService);
    }

    public static TRXLanguageServices CreateTRX()
    {
        var lineService = new TRXLineService();
        var documentService = new TRXDocumentService(lineService);
        var schemaService = new TRXGameFlowSchemaService(TRXResourcePaths.GetGameFlowSchemaPath());

        return new TRXLanguageServices(
            schemaService,
            lineService,
            documentService,
            new TRXDefinitionProvider(documentService),
            new TRXGameFlowCompletionService(schemaService),
            new TRXGameFlowHoverService(schemaService));
    }
}