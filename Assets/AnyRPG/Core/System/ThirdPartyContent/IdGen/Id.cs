using System;

namespace IdGen {

    /// <summary>
    /// Holds information about a decoded id.
    /// </summary>
    public readonly struct Id : IEquatable<Id> {
        /// <summary>
        /// Gets the sequence number of the id.
        /// </summary>
        public int SequenceNumber { get; }

        /// <summary>
        /// Gets the generator id of the generator that generated the id.
        /// </summary>
        public int GeneratorId { get; }

        /// <summary>
        /// Gets the date/time when the id was generated.
        /// </summary>
        public DateTimeOffset DateTimeOffset { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Id"/> struct.
        /// </summary>
        /// <param name="sequenceNumber">The sequence number of the id.</param>
        /// <param name="generatorId">The generator id of the generator that generated the id.</param>
        /// <param name="dateTimeOffset">The date/time when the id was generated.</param>
        /// <returns>An <see cref="Id"/>.</returns>
        internal Id(int sequenceNumber, int generatorId, DateTimeOffset dateTimeOffset) {
            SequenceNumber = sequenceNumber;
            GeneratorId = generatorId;
            DateTimeOffset = dateTimeOffset;
        }

        // Records automatically implement value equality; for a standard struct, we do it manually:
        public override bool Equals(object obj) => obj is Id other && Equals(other);
        public bool Equals(Id other) =>
            SequenceNumber == other.SequenceNumber &&
            GeneratorId == other.GeneratorId &&
            DateTimeOffset == other.DateTimeOffset;

        public override int GetHashCode() =>
            HashCode.Combine(SequenceNumber, GeneratorId, DateTimeOffset);

        public static bool operator ==(Id left, Id right) => left.Equals(right);
        public static bool operator !=(Id left, Id right) => !left.Equals(right);

        public override string ToString() =>
            $"Id {{ SequenceNumber = {SequenceNumber}, GeneratorId = {GeneratorId}, DateTimeOffset = {DateTimeOffset} }}";
    }
}