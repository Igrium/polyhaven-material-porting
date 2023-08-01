package com.igrium.metadata_server.asset;

import java.util.List;
import java.util.Map;

import com.google.gson.annotations.SerializedName;

public record AssetEntry(
        @SerializedName("Name") String name,

        @SerializedName("files_hash") String filesHash,

        @SerializedName("Authors") Map<String, String> authors,

        @SerializedName("Categories") List<String> categories,

        @SerializedName("Tags") List<String> tagsList,

        @SerializedName("Dimensions") String dimensions) {

}
