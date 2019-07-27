using System.Collections.Generic;

namespace Gw2Sharp.WebApi.V2.Models
{
    /// <summary>
    /// Represents the profession weapon details.
    /// </summary>
    public class ProfessionWeapon
    {
        /// <summary>
        /// The specialization that unlocks this weapon for the profession.
        /// If no specialization is required, this value is <c>null</c>.
        /// Can be resolved against <see cref="IGw2WebApiV2Client.Specializations"/>.
        /// </summary>
        public int Specialization { get; set; }

        /// <summary>
        /// The profession weapon flags.
        /// </summary>
        public ApiFlags<ProfessionWeaponFlag> Flags { get; set; }

        /// <summary>
        /// The profession weapon skills.
        /// </summary>
        public IReadOnlyList<ProfessionWeaponSkill> Skills { get; set; } = new List<ProfessionWeaponSkill>();
    }
}
