using DataLibrary.JsonConverters;
using System.Text.Json.Serialization;

namespace DataLibrary.Dtos;

public class AddressDto
{
    [JsonPropertyName("account_id_mdp_field_acctid")]
    public string? AccountId { get; set; }

    [JsonPropertyName("record_key_account_number_sdat_field_3")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("record_key_district_ward_sdat_field_2")]
    public string? Ward { get; set; }

    [JsonPropertyName("section_mdp_field_section_sdat_field_39")]
    public string? Section { get; set; }

    [JsonPropertyName("block_mdp_field_block_sdat_field_40")]
    public string? Block { get; set; }

    [JsonPropertyName("lot_mdp_field_lot_sdat_field_41")]
    public string? Lot { get; set; }

    [JsonPropertyName("land_use_code_mdp_field_lu_desclu_sdat_field_50")]
    [JsonConverter(typeof(LandUseCodeJsonConverter))]
    public string? LandUseCode { get; set; }

    [JsonPropertyName("c_a_m_a_system_data_year_built_yyyy_mdp_field_yearblt_sdat_field_235")]
    [JsonConverter(typeof(IntJsonConverter))]
    public int? YearBuilt { get; set; }
}
