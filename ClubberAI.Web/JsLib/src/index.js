import { setupWalletSelector } from "@near-wallet-selector/core";
import { setupModal } from "@near-wallet-selector/modal-ui";
import { setupMyNearWallet } from "@near-wallet-selector/my-near-wallet";
import { setupHereWallet } from "@near-wallet-selector/here-wallet";
import { Contract } from "near-api-js";

const nearConfig = {
  networkId: 'testnet',
  nodeUrl: 'https://rpc.testnet.near.org',
  walletUrl: 'https://wallet.testnet.near.org',
};

let selector;
let modal;

export async function initNear() {
  selector = await setupWalletSelector({
    network: "testnet",
    modules: [setupMyNearWallet(), setupHereWallet()],
  });

  modal = setupModal(selector, {
    contractId: "club.tkn.primitives.testnet",
  });
}

export function signIn() {
  modal.show();
}

export async function signOut() {
  const { selectedWalletId } = selector.store.getState();
  const walletModule = await selector.wallet(selectedWalletId);
  if (walletModule) {
    await walletModule.signOut();
  }
}

export function isSignedIn() {
  return selector.isSignedIn();
}

export function getAccountId() {
  const { accounts } = selector.store.getState();
  return accounts.length > 0 ? accounts[0].accountId : null;
}

export async function getTokenBalance() {
  if (!isSignedIn()) {
    throw new Error('User is not signed in');
  }
  const accountId = getAccountId();
  if (!accountId) {
    throw new Error('No account ID found');
  }
  const { selectedWalletId } = selector.store.getState();
  const walletModule = await selector.wallet(selectedWalletId);
  const account = await walletModule.getAccounts();

  const contract = new Contract(account[0], 'club.tkn.primitives.testnet', {
    viewMethods: ['ft_balance_of'],
    changeMethods: [],
  });

  return await contract.ft_balance_of({ account_id: accountId });
}
