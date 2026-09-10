using Xunit;

// Each test class here spins up its own WebApplicationFactory<Program>, and all of
// them point at the same on-disk SQLite dev database (see appsettings.json's "AppDb"
// connection string). Running these classes in parallel means multiple hosts race to
// migrate/seed the same file concurrently. Under SQLite's classic rollback-journal
// mode (forced in Program.cs to work around OneDrive/WAL incompatibility) this
// produces file-lock contention that can hang instead of failing fast. Disabling
// parallelization keeps the test suite fast enough to still be useful while avoiding
// that contention.
[assembly: CollectionBehavior(DisableTestParallelization = true)]
