using NLog;
using System.Text.Json;

namespace TombLib.LanguageServer.Core.Tests;

public partial class LanguageServerClientTests
{
	[TestMethod]
	public void BuildConfigurationResponse_ReturnsRequestedLuaSections()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new
		{
			Lua = new
			{
				runtime = new
				{
					version = "Lua 5.4"
				}
			}
		}));

		object[] response = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua"),
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.AreEqual("Lua 5.4", JsonSerializer.SerializeToElement(response[1]).GetProperty("version").GetString());
	}

	[TestMethod]
	public void BuildConfigurationResponse_ReturnsRequestedNestedNonLuaSections()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new
		{
			Editor = new
			{
				theme = "Dark",
				fonts = new
				{
					size = 14
				}
			}
		}));

		object[] response = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Editor"),
				new WorkspaceConfigurationItem("Editor.fonts")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.AreEqual("Dark", JsonSerializer.SerializeToElement(response[0]).GetProperty("theme").GetString());
		Assert.AreEqual(14, JsonSerializer.SerializeToElement(response[1]).GetProperty("size").GetInt32());
	}

	[TestMethod]
	public void BuildConfigurationResponse_ReturnsNullWhenLuaSectionIsMissing()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new
		{
			Editor = new
			{
				theme = "Dark"
			}
		}));

		object[] response = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua"),
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.IsNull(response[0]);
		Assert.IsNull(response[1]);
	}

	[TestMethod]
	public void BuildConfigurationResponse_ReusesCachedSettingsSnapshotAcrossRepeatedRequests()
	{
		int settingsProviderCallCount = 0;

		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(() =>
		{
			settingsProviderCallCount++;

			return new
			{
				Lua = new
				{
					Runtime = new
					{
						Version = "Lua 5.4"
					}
				}
			};
		}));

		object[] firstResponse = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		object[] secondResponse = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(1, settingsProviderCallCount);
		Assert.AreEqual("Lua 5.4", JsonSerializer.SerializeToElement(firstResponse[0]).GetProperty("version").GetString());
		Assert.AreEqual("Lua 5.4", JsonSerializer.SerializeToElement(secondResponse[0]).GetProperty("version").GetString());
	}

	[TestMethod]
	public async Task SendNotificationAsync_WhenDidChangeConfigurationIsAlreadyCanceled_PreservesCachedSettingsSnapshot()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new
		{
			Lua = new
			{
				Runtime = new
				{
					Version = "Lua 5.4"
				}
			}
		}));

		using var cancellationSource = new CancellationTokenSource();
		object session = CreateTransportSession(client, 11, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		SetReadyState(client, true);
		cancellationSource.Cancel();

		object[] initialResponse = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual("Lua 5.4", JsonSerializer.SerializeToElement(initialResponse[0]).GetProperty("version").GetString());

		await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
			await client.SendNotificationAsync(
				"workspace/didChangeConfiguration",
				new DidChangeConfigurationParams(new
				{
					Lua = new
					{
						Runtime = new
						{
							Version = "Lua 5.1"
						}
					}
				}),
				cancellationSource.Token).ConfigureAwait(false)).ConfigureAwait(false);

		object[] responseAfterFailure = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual("Lua 5.4", JsonSerializer.SerializeToElement(responseAfterFailure[0]).GetProperty("version").GetString());
		Assert.IsTrue(client.IsReady);
		Assert.AreEqual(11L, client.TransportGeneration);
	}

	[TestMethod]
	public void BuildConfigurationResponse_TypedSettingsObject_MatchesNestedSectionCaseInsensitively()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new TestConfigurationRoot
		{
			Lua = new TestLuaConfiguration
			{
				Runtime = new TestLuaRuntimeConfiguration
				{
					Version = "Lua 5.4"
				}
			}
		}));

		object[] response = (object[])InvokePrivateMethodWithReturn(client, "BuildConfigurationResponse",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(1, response.Length);
		Assert.IsNotNull(response[0]);
		Assert.AreEqual("Lua 5.4", JsonSerializer.SerializeToElement(response[0]).GetProperty("version").GetString());
	}

	[TestMethod]
	public void WorkspaceConfiguration_WhenSettingsSerializationFails_ReturnsNullValuesAndLogsWarning()
	{
		using var logScope = new NLogMemoryScope(LogLevel.Warn);

		var cyclicSettings = new Dictionary<string, object?>();
		cyclicSettings["self"] = cyclicSettings;

		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(() => cyclicSettings));
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		object[] response = (object[])InvokePrivateMethodWithReturn(rpcTarget,
			"WorkspaceConfiguration",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua"),
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.IsNull(response[0]);
		Assert.IsNull(response[1]);

		Assert.IsTrue(logScope.Logs.Any(log => log.StartsWith("Warn|", StringComparison.Ordinal)
			&& log.Contains("workspace/configuration", StringComparison.OrdinalIgnoreCase)
			&& log.Contains("returning null values", StringComparison.OrdinalIgnoreCase)),
			string.Join(Environment.NewLine, logScope.Logs));
	}

	[TestMethod]
	public void BuildInitializeParams_UsesInjectedInitializationOptions()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			ClientCapabilitiesProvider = static _ => new { },
			InitializationOptionsProvider = static workspaceRoot => new
			{
				workspace = workspaceRoot,
				customFlag = true
			}
		});

		JsonElement initializeParams = JsonSerializer.SerializeToElement(InvokePrivateMethodWithReturn(client, "BuildInitializeParams"));
		JsonElement initializationOptions = initializeParams.GetProperty("initializationOptions");

		Assert.AreEqual(@"C:\Workspace", initializationOptions.GetProperty("workspace").GetString());
		Assert.IsTrue(initializationOptions.GetProperty("customFlag").GetBoolean());
	}

	[TestMethod]
	public void BuildInitializeParams_UsesInjectedClientCapabilities()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			ClientCapabilitiesProvider = static workspaceRoot => new
			{
				workspace = new
				{
					workspaceFolders = true,
					configuration = true,
					root = workspaceRoot
				}
			}
		});

		JsonElement initializeParams = JsonSerializer.SerializeToElement(InvokePrivateMethodWithReturn(client, "BuildInitializeParams"));
		JsonElement capabilities = initializeParams.GetProperty("capabilities");

		Assert.IsTrue(capabilities.GetProperty("workspace").GetProperty("workspaceFolders").GetBoolean());
		Assert.IsTrue(capabilities.GetProperty("workspace").GetProperty("configuration").GetBoolean());
		Assert.AreEqual(@"C:\Workspace", capabilities.GetProperty("workspace").GetProperty("root").GetString());
	}

	[TestMethod]
	public void BuildInitializeParams_ForcesUnsupportedDynamicRegistrationFlagsToFalse()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			ClientCapabilitiesProvider = static _ => new
			{
				workspace = new
				{
					didChangeWatchedFiles = new { dynamicRegistration = true }
				},
				textDocument = new
				{
					rename = new { dynamicRegistration = true, prepareSupport = true },
					references = new { dynamicRegistration = true },
					formatting = new { dynamicRegistration = true },
					completion = new { dynamicRegistration = true }
				}
			}
		});

		JsonElement initializeParams = JsonSerializer.SerializeToElement(InvokePrivateMethodWithReturn(client, "BuildInitializeParams"));
		JsonElement capabilities = initializeParams.GetProperty("capabilities");

		Assert.IsFalse(capabilities.GetProperty("workspace").GetProperty("didChangeWatchedFiles").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsFalse(capabilities.GetProperty("textDocument").GetProperty("rename").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsFalse(capabilities.GetProperty("textDocument").GetProperty("references").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsFalse(capabilities.GetProperty("textDocument").GetProperty("formatting").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsFalse(capabilities.GetProperty("textDocument").GetProperty("completion").GetProperty("dynamicRegistration").GetBoolean());
		Assert.IsTrue(capabilities.GetProperty("textDocument").GetProperty("rename").GetProperty("prepareSupport").GetBoolean());
	}

	[TestMethod]
	public void BuildInitializeParams_NormalizesWorkspaceRootAndFolderName()
	{
		using var client = new LanguageServerClient(@"C:/Workspace/", "lua-language-server.exe", new LanguageServerClientOptions(static () => new { })
		{
			ClientCapabilitiesProvider = static _ => new { },
			InitializationOptionsProvider = static workspaceRoot => new
			{
				workspace = workspaceRoot
			}
		});

		JsonElement initializeParams = JsonSerializer.SerializeToElement(InvokePrivateMethodWithReturn(client, "BuildInitializeParams"));
		JsonElement workspaceFolder = initializeParams.GetProperty("workspaceFolders")[0];
		string expectedWorkspaceRoot = LanguageServerPathHelper.NormalizeLocalPath(@"C:/Workspace/");

		Assert.AreEqual(expectedWorkspaceRoot, initializeParams.GetProperty("initializationOptions").GetProperty("workspace").GetString());
		Assert.AreEqual(LanguageServerPathHelper.CreateFileUri(expectedWorkspaceRoot), initializeParams.GetProperty("rootUri").GetString());
		Assert.AreEqual(LanguageServerPathHelper.CreateFileUri(expectedWorkspaceRoot), workspaceFolder.GetProperty("uri").GetString());
		Assert.AreEqual("Workspace", workspaceFolder.GetProperty("name").GetString());
	}

	[TestMethod]
	public void WorkspaceFolders_ReturnsDriveRootNameWhenWorkspaceRootIsDriveRoot()
	{
		using var client = new LanguageServerClient(@"C:\", "lua-language-server.exe", DefaultClientOptions);
		object session = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, session);
		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(session));

		WorkspaceFolder[] workspaceFolders = (WorkspaceFolder[])InvokePrivateMethodWithReturn(rpcTarget, "WorkspaceFolders");

		Assert.AreEqual(1, workspaceFolders.Length);
		Assert.AreEqual(LanguageServerPathHelper.CreateFileUri(@"C:\"), workspaceFolders[0].Uri);
		Assert.AreEqual("C:", workspaceFolders[0].Name);
	}

	[TestMethod]
	public void WorkspaceConfiguration_StaleTransportGeneration_ReturnsNullValuesWithoutReadingActiveSettings()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", new LanguageServerClientOptions(static () => new
		{
			Lua = new
			{
				Runtime = new
				{
					Version = "5.4"
				}
			}
		}));

		object staleSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object activeSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, activeSession);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(staleSession));
		object[] response = (object[])InvokePrivateMethodWithReturn(rpcTarget,
			"WorkspaceConfiguration",
			new WorkspaceConfigurationParams(
			[
				new WorkspaceConfigurationItem("Lua"),
				new WorkspaceConfigurationItem("Lua.runtime")
			]));

		Assert.AreEqual(2, response.Length);
		Assert.IsNull(response[0]);
		Assert.IsNull(response[1]);
	}

	[TestMethod]
	public void WorkspaceFolders_StaleTransportGeneration_ReturnsEmptyArray()
	{
		using var client = new LanguageServerClient(@"C:\Workspace", "lua-language-server.exe", DefaultClientOptions);
		object staleSession = CreateTransportSession(client, 1, process: null, Stream.Null, Stream.Null);
		object activeSession = CreateTransportSession(client, 2, process: null, Stream.Null, Stream.Null);

		SetActiveSession(client, activeSession);

		object rpcTarget = CreateRpcTarget(client, GetTransportGeneration(staleSession));
		WorkspaceFolder[] workspaceFolders = (WorkspaceFolder[])InvokePrivateMethodWithReturn(rpcTarget, "WorkspaceFolders");

		Assert.AreEqual(0, workspaceFolders.Length);
	}
}
