// <copyright file="checkbox-wrapper.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import * as React from "react";
import { Flex, RadioGroup } from "@fluentui/react-northstar";

export interface IRadioGroupItem {
	name: string;
	key: string;
	label: string;
	value: string;
}

interface IRadioGroupProps {
	radioItems: Array<IRadioGroupItem>,
	selectedItem: string,
	onChange: (value: string) => void
}

interface IRadioGroupState {
	selectedItem: string
}

class RadioGroupWrapper extends React.Component<IRadioGroupProps, IRadioGroupState> {
	constructor(props) {
		super(props);

		this.state = {
			selectedItem: this.props.selectedItem
		};
	}

	componentWillReceiveProps(nextProps: IRadioGroupProps) {
		if (nextProps.selectedItem !== this.props.selectedItem) {
			this.setState({ selectedItem: nextProps.selectedItem })
		}
	}

	/**
    * Renders the component
    */
	public render(): JSX.Element {
		return (
			<Flex gap="gap.small">
				<RadioGroup
					defaultCheckedValue={this.state.selectedItem}
					vertical
					items={this.props.radioItems}
					onCheckedValueChange={(event, data: any) => this.props.onChange(data.value)}
				/>
			</Flex>
		);
	}
}

export default RadioGroupWrapper;