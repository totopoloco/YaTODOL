using Xunit;

// Tests that depend on the global static Strings.Language must not run in parallel
// with StringsTests, which mutates that state as part of its language-switching tests.
[assembly: CollectionBehavior(DisableTestParallelization = false)]

namespace YATODOL.Tests;

/// <summary>
/// Sequential collection for tests that rely on shared static state in <c>Strings</c>.
/// Members of this collection run one at a time, preventing race conditions on the
/// global language setting.
/// </summary>
[CollectionDefinition("Strings-dependent", DisableParallelization = true)]
public class StringsDependentCollection { }
