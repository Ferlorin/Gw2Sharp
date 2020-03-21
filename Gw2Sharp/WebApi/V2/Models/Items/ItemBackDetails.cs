using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Gw2Sharp.Json.Converters;

namespace Gw2Sharp.WebApi.V2.Models
{
    /// <summary>
    /// Represents the details of a backpiece item.
    /// </summary>
    public class ItemBackDetails
    {
        /// <summary>
        /// The item infusion slots.
        /// </summary>
        public IReadOnlyList<ItemInfusionSlot> InfusionSlots { get; set; } = Array.Empty<ItemInfusionSlot>();

        /// <summary>
        /// The item infix upgrade.
        /// If the item does not have a infix upgrade, this value is <c>null</c>.
        /// </summary>
        public ItemInfixUpgrade? InfixUpgrade { get; set; }

        /// <summary>
        /// The id of the suffix item.
        /// If the item does not have a suffix item, this value is <c>null</c>.
        /// </summary>
        public int? SuffixItemId { get; set; }

        /// <summary>
        /// The id of the secondary suffix item.
        /// If the item does not have a secondary suffix item, this value is <c>null</c>.
        /// </summary>
        [JsonConverter(typeof(StringAsIntConverter))]
        public int? SecondarySuffixItemId { get; set; }

        /// <summary>
        /// The item stat choices.
        /// Each element can be resolved against <see cref="IGw2WebApiV2Client.Itemstats"/>.
        /// </summary>
        public IReadOnlyList<int> StatChoices { get; set; } = Array.Empty<int>();
    }
}
