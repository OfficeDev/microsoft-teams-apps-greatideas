// <copyright file="index.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Suspense } from "react";
import * as ReactDOM from "react-dom";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import App from "./app";


ReactDOM.render(
	<React.StrictMode>
	<Suspense fallback={<div className="container-div"><div className="container-subdiv"></div></div>}>
	<BrowserRouter>
		<Routes>
			<Route path="*" element={<App />}>
			</Route>
		</Routes>
		</BrowserRouter>
	</Suspense>
</React.StrictMode>, document.getElementById("root"));