﻿import * as React from "react";
import { Text, Button } from "@fluentui/react-northstar";
import * as microsoftTeams from "@microsoft/teams-js";
import { useTranslation } from 'react-i18next';
import "../../styles/signin.css";

const SignInPage: React.FunctionComponent<any> = props => {
    const localize = useTranslation().t;
    const errorMessage = "Please sign in to continue.";

    function onSignIn() {
        microsoftTeams.initialize();
        microsoftTeams.authentication.authenticate({
            url: window.location.origin + "/signin-simple-start",
            successCallback: () => {
                console.log("Login succeeded!");
                window.location.href = "/discover";
            },
            failureCallback: (reason) => {
                console.log("Login failed: " + reason);
                window.location.href = "/errorpage";
            }
        });
    }

    return (
        <div className="sign-in-content-container">
            <div>
            </div>
            <Text
                content={errorMessage}
                size="medium"
            />
            <div className="space"></div>
            <Button content={localize("signInText")} primary className="sign-in-button" onClick={onSignIn} />
        </div>
    );
};

export default SignInPage;
