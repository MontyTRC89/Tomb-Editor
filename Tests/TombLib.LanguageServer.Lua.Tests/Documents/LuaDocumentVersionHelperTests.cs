namespace Nickelony.LanguageServer.Lua.Tests;

[TestClass]
public class LuaDocumentVersionHelperTests
{
	[TestMethod]
	public void TryAccept_RejectsOlderPositiveVersion()
	{
		bool accepted = LuaDocumentVersionHelper.TryAccept(currentVersion: 5, incomingVersion: 4, out int acceptedVersion);

		Assert.IsFalse(accepted);
		Assert.AreEqual(5, acceptedVersion);
	}

	[TestMethod]
	public void TryAccept_PreservesCurrentVersionForUnversionedPayload()
	{
		bool accepted = LuaDocumentVersionHelper.TryAccept(currentVersion: 5, incomingVersion: 0, out int acceptedVersion);

		Assert.IsTrue(accepted);
		Assert.AreEqual(5, acceptedVersion);
	}

	[TestMethod]
	public void TryAccept_AdvancesToNewerPositiveVersion()
	{
		bool accepted = LuaDocumentVersionHelper.TryAccept(currentVersion: 5, incomingVersion: 6, out int acceptedVersion);

		Assert.IsTrue(accepted);
		Assert.AreEqual(6, acceptedVersion);
	}
}
