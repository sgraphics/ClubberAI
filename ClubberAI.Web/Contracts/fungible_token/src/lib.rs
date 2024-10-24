use near_sdk::{
    PanicOnDefault, borsh::{BorshDeserialize, BorshSerialize, self}, env,
    near_bindgen,
};
use near_sdk_contract_tools::{Owner, ft::*, owner::{*, hooks::OnlyOwner}};

#[derive(BorshSerialize, BorshDeserialize, PanicOnDefault, Owner, FungibleToken)]
#[fungible_token(mint_hook = "OnlyOwner")]
#[near_bindgen]
pub struct Contract {}

#[near_bindgen]
impl Contract {
    #[init]
    pub fn new() -> Self {
        let mut contract = Self {};

        Owner::init(&mut contract, &env::predecessor_account_id());
        Nep148Controller::set_metadata(
            &mut contract,
            &FungibleTokenMetadata::new("ClubberAI Token".to_string(), "CLUB".to_string(), 24),
        );

        contract
    }
}
