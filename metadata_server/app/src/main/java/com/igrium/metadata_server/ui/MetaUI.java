package com.igrium.metadata_server.ui;

import java.io.IOException;

import com.igrium.metadata_server.TextUtil;
import com.igrium.metadata_server.asset.AssetMeta;

import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.scene.Parent;
import javafx.scene.control.Button;
import javafx.scene.control.Label;
import javafx.scene.control.TextArea;
import javafx.scene.control.TextField;
import javafx.util.Pair;

public class MetaUI {

    protected AssetMeta asset;

    public AssetMeta getAsset() {
        return asset;
    }

    @FXML
    private Label title;

    public Label getTitle() {
        return title;
    }

    @FXML
    private TextArea description;

    public TextArea getDescription() {
        return description;
    }

    @FXML
    private TextField tags;

    public TextField getTags() {
        return tags;
    }

    @FXML
    private Button doneButton;

    public Button getDoneButton() {
        return doneButton;
    }

    @FXML
    private Button openButton;

    public Button getOpenButton() {
        return openButton;
    }

    @FXML
    protected void openWeb() {
        AppUI.getInstance().getHostServices().showDocument(asset.assetPartyURL());
    }

    public void Load(AssetMeta asset) {
        title.setText(asset.assetEntry().name());
        description.setText(TextUtil.writeDescription(asset));
        tags.setText(TextUtil.writeTags(asset.assetEntry().tagsList()));
        this.asset = asset;
    }

    public static Pair<MetaUI, Parent> create() {
        FXMLLoader loader = new FXMLLoader(MetaUI.class.getResource("/ui/meta.fxml"));

        Parent ui;
        try {
            ui = loader.load();
        } catch (IOException e) {
            throw new RuntimeException(e);
        }

        MetaUI controller = loader.getController();

        return new Pair<>(controller, ui);
    }
}
