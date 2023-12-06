using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;

namespace GeneticSharp
{
    /// <summary>
    /// Integer chromosome with binary values (0 and 1).
    /// </summary>
    public class FastIntegerChromosome : IBinaryChromosome
    {
        private static readonly object Boxed0 = 0;
        private static readonly object Boxed1 = 1;

        private readonly Gene Gene0 = new(Boxed0);
        private readonly Gene Gene1 = new(Boxed1);

        private readonly int m_minValue;
        private readonly int m_maxValue;
        private readonly int m_originalValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:GeneticSharp.Domain.Chromosomes.IntegerChromosome"/> class.
        /// </summary>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Maximum value.</param>
        public FastIntegerChromosome(int minValue, int maxValue)
        {
            m_minValue = minValue;
            m_maxValue = maxValue;
            var intValue = RandomizationProvider.Current.GetInt(m_minValue, m_maxValue);
            m_originalValue = intValue;

            CreateGenes();
        }

        private static int GetBit(int val, int bit)
        {
            return (val >> bit) & 1;
        }

        /// <summary>
        /// Generates the gene.
        /// </summary>
        /// <returns>The gene.</returns>
        /// <param name="geneIndex">Gene index.</param>
        public Gene GenerateGene(int geneIndex)
        {
            var value = GetBit(m_originalValue, geneIndex);

            return (value == 0) ? Gene0 : Gene1;
        }

        /// <summary>
        /// Creates the new.
        /// </summary>
        /// <returns>The new.</returns>
        public IChromosome CreateNew()
        {
            return new FastIntegerChromosome(m_minValue, m_maxValue);
        }

        public int ToInteger()
        {
            var genes = GetGenes();
            Debug.Assert(genes.Length == 32);
            var val = 0;

            for (var i = 0; i < genes.Length; i++)
            {
                val += ((int)genes[i].Value) << i;
            }

            return val;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:GeneticSharp.Domain.Chromosomes.FloatingPointChromosome"/>.
        /// </summary>
        /// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:GeneticSharp.Domain.Chromosomes.FloatingPointChromosome"/>.</returns>
        public override string ToString()
        {
            return String.Join("", GetGenes().Reverse().Select(g => (int) g.Value).ToArray());
        }

        /// <summary>
        /// Flips the gene.
        /// </summary>
        /// <remarks>>
        /// If gene's value is 0, the it will be flip to 1 and vice-versa.</remarks>
        /// <param name="index">The gene index.</param>
        public void FlipGene(int index)
        {
            var realIndex = Math.Abs(31 - index);
            var value = object.ReferenceEquals(GetGene(realIndex).Value, Gene0.Value);

            ReplaceGene(realIndex, value ? Gene1 : Gene0);
        }


        #region Fields
        private readonly Gene[] m_genes = new Gene[m_length];
        private const int m_length = 32;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the fitness of the chromosome in the current problem.
        /// </summary>
        public double? Fitness { get; set; }

        /// <summary>
        /// Gets the length, in genes, of the chromosome.
        /// </summary>
        public int Length => m_length;
        #endregion

        #region Methods
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(FastIntegerChromosome first, FastIntegerChromosome second)
        {
            if (Object.ReferenceEquals(first, second))
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            return first.CompareTo(second) == 0;
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(FastIntegerChromosome first, FastIntegerChromosome second)
        {
            return !(first == second);
        }

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <(FastIntegerChromosome first, FastIntegerChromosome second)
        {
            if (Object.ReferenceEquals(first, second))
            {
                return false;
            }

            if ((object)first == null)
            {
                return true;
            }

            if ((object)second == null)
            {
                return false;
            }

            return first.CompareTo(second) < 0;
        }

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >(FastIntegerChromosome first, FastIntegerChromosome second)
        {
            return !(first == second) && !(first < second);
        }

        /// <summary>
        /// Creates a clone.
        /// </summary>
        /// <returns>The chromosome clone.</returns>
        public virtual IChromosome Clone()
        {
            var clone = CreateNew();
            clone.ReplaceGenes(0, GetGenes());
            clone.Fitness = Fitness;

            return clone;
        }

        /// <summary>
        /// Replaces the gene in the specified index.
        /// </summary>
        /// <param name="index">The gene index to replace.</param>
        /// <param name="gene">The new gene.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index;There is no Gene on index {0} to be replaced..With(index)</exception>
        public void ReplaceGene(int index, Gene gene)
        {
            if (index < 0 || index >= m_length)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "There is no Gene on index {0} to be replaced.".With(index));
            }

            m_genes[index] = gene;
            Fitness = null;
        }

        /// <summary>
        /// Replaces the genes starting in the specified index.
        /// </summary>
        /// <param name="startIndex">Start index.</param>
        /// <param name="genes">The genes.</param>
        /// <remarks>
        /// The genes to be replaced can't be greater than the available space between the start index and the end of the chromosome.
        /// </remarks>
        public void ReplaceGenes(int startIndex, Gene[] genes)
        {
            ExceptionHelper.ThrowIfNull("genes", genes);

            if (genes.Length > 0)
            {
                if (startIndex < 0 || startIndex >= m_length)
                {
                    throw new ArgumentOutOfRangeException(nameof(startIndex), "There is no Gene on index {0} to be replaced.".With(startIndex));
                }

                Array.Copy(genes, 0, m_genes, startIndex, Math.Min(genes.Length, m_length - startIndex));

                Fitness = null;
            }
        }

        /// <summary>
        /// Gets the gene in the specified index.
        /// </summary>
        /// <param name="index">The gene index.</param>
        /// <returns>
        /// The gene.
        /// </returns>
        public Gene GetGene(int index)
        {
            return m_genes[index];
        }

        /// <summary>
        /// Gets the genes.
        /// </summary>
        /// <returns>The genes.</returns>
        public Gene[] GetGenes()
        {
            return m_genes;
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <returns>The to.</returns>
        /// <param name="other">The other chromosome.</param>
        public int CompareTo(IChromosome other)
        {
            if (other == null)
            {
                return -1;
            }

            var otherFitness = other.Fitness;

            if (Fitness == otherFitness)
            {
                return 0;
            }

            return Fitness > otherFitness ? 1 : -1;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to the current <see cref="GeneticSharp.ChromosomeBase"/>.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with the current <see cref="GeneticSharp.ChromosomeBase"/>.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object"/> is equal to the current
        /// <see cref="GeneticSharp.ChromosomeBase"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var other = obj as IChromosome;

            if (other == null)
            {
                return false;
            }

            return CompareTo(other) == 0;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return Fitness.GetHashCode();
        }

        /// <summary>
        /// Creates the gene on specified index.
        /// <remarks>
        /// It's a shortcut to:  
        /// <code>
        /// ReplaceGene(index, GenerateGene(index));
        /// </code>
        /// </remarks>
        /// </summary>
        /// <param name="index">The gene index.</param>
        protected virtual void CreateGene(int index)
        {
            ReplaceGene(index, GenerateGene(index));
        }

        /// <summary>
        /// Creates all genes
        /// <remarks>
        /// It's a shortcut to: 
        /// <code>
        /// for (int i = 0; i &lt; Length; i++)
        /// {
        ///     ReplaceGene(i, GenerateGene(i));
        /// }
        /// </code>
        /// </remarks>
        /// </summary>        
        protected virtual void CreateGenes()
        {
            for (int i = 0; i < Length; i++)
            {
                ReplaceGene(i, GenerateGene(i));
            }
        }

        public void Resize(int newLength)
        {
            if (newLength != 32)
                throw new NotSupportedException();
        }

        #endregion
    }
}

