namespace Nickelony.LanguageServer.Core.Tests;

internal sealed class TestTrackedDocumentStore : TrackedDocumentStore<TestTrackedDocumentState>
{
	protected override TestTrackedDocumentState CreateTrackedDocumentState(
		string filePath,
		string uri,
		string content,
		int version,
		bool isOpen,
		int openReferenceCount,
		int requestReferenceCount,
		long lastAccessStamp)
		=> new(filePath, uri, content, version, isOpen, openReferenceCount, requestReferenceCount, lastAccessStamp);

	protected override long GetLastAccessStamp(TestTrackedDocumentState state)
		=> state.LastAccessStamp;

	protected override void TouchTrackedDocumentState(TestTrackedDocumentState state, long lastAccessStamp)
		=> state.Touch(lastAccessStamp);

	protected override void ReopenTrackedDocumentState(TestTrackedDocumentState state, string content)
		=> state.Reopen(content);

	protected override string ReplaceTrackedDocumentContent(TestTrackedDocumentState state, string content)
		=> state.Update(content);

	protected override void RenameTrackedDocumentState(TestTrackedDocumentState state, string filePath, string uri)
		=> state.Rename(filePath, uri);

	protected override void MarkTrackedDocumentClosed(TestTrackedDocumentState state)
		=> state.Close();
}

internal sealed class TestTrackedDocumentState : TrackedDocumentState
{
	public TestTrackedDocumentState(string filePath, string uri, string content, int version, bool isOpen,
		int openReferenceCount, int requestReferenceCount, long lastAccessStamp)
		: base(filePath, uri, content, version, isOpen, openReferenceCount, requestReferenceCount, lastAccessStamp)
	{ }

	public void Touch(long lastAccessStamp)
		=> SetLastAccessStamp(lastAccessStamp);

	public void Reopen(string content)
		=> ReopenDocument(content);

	public string Update(string content)
		=> ReplaceContent(content);

	public void Rename(string filePath, string uri)
		=> RenameDocument(filePath, uri);

	public void Close()
		=> MarkDocumentClosed();
}
