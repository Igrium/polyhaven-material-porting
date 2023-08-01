package metadata_server;

import static org.junit.Assert.assertEquals;

import java.util.Collections;
import java.util.List;

import org.junit.Test;

import com.igrium.metadata_server.TextUtil;

public class TestText {
    
    @Test
    public void testTags() {
        assertEquals("test test1 test_space afterspace", TextUtil.writeTags(List.of("test", "test1", "test_space", "afterspace")));
    }

    @Test
    public void testAuthors() {
        assertEquals("paul", TextUtil.writeAuthors(List.of("paul")));
        assertEquals("paul and sam", TextUtil.writeAuthors(List.of("paul", "sam")));
        assertEquals("paul, sam, and rose", TextUtil.writeAuthors(List.of("paul", "sam", "rose")));
        assertEquals("", TextUtil.writeAuthors(Collections.emptyList()));
    }
}
