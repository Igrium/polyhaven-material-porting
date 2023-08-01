package com.igrium.metadata_server;

import java.util.ArrayList;
import java.util.Collection;
import java.util.Iterator;
import java.util.List;

import com.igrium.metadata_server.asset.AssetMeta;

public class TextUtil {
    private TextUtil() {};

    public static String writeTags(Collection<String> tags) {
        var newTags = tags.stream().map(tag -> tag.replace(' ', '_'));
        String str = "";
        Iterator<String> iter = newTags.iterator();
        
        while (iter.hasNext()) {
            str += iter.next();
            if (iter.hasNext()) str += " ";
        }

        return str;
    }

    public static String writeAuthors(Iterable<String> authors) {
        List<String> list = new ArrayList<>();
        authors.forEach(list::add);
        if (list.size() == 0) return "";
        if (list.size() == 1) return list.get(0);
        if (list.size() == 2) return list.get(0) + " and " + list.get(1);

        String str = "";

        for (int i = 0; i < list.size() - 1; i++) {
            str += list.get(i) + ", ";
        }
        str += "and " + list.get(list.size() - 1);
        return str;
    }

    public static String writeDescription(AssetMeta asset) {
        return String.format("By %s on PolyHaven \n \n %s",
                writeAuthors(asset.assetEntry().authors().keySet()),
                writeURL(asset.polyID()));
    }
    
    public static String writeURL(String polyID) {
        return "https://polyhaven.com/a/" + polyID;
    }
}